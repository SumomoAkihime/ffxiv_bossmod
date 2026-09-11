"""生成或比较本地雷达源码快照；仅使用 Python 标准库，不修改保护文件。"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")
sys.stderr.reconfigure(encoding="utf-8")

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "sync/local-radars.json"


def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])


def snapshot(revision):
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8-sig"))
    entries = manifest["modules"] + manifest["supportFiles"]
    prefixes = []
    for entry in entries:
        path = entry["path"]
        target = (ROOT / path).resolve()
        if not target.is_relative_to(ROOT) or path in prefixes:
            raise ValueError(f"保护路径重复或越界：{path}")
        prefixes.append(path)
    files = {}
    if revision:
        names = git("ls-tree", "-r", "--name-only", revision).decode("utf-8").splitlines()
    else:
        names = []
        for prefix in prefixes:
            target = ROOT / prefix
            if not target.exists():
                raise ValueError(f"保护路径缺失：{prefix}")
            names.extend(p.relative_to(ROOT).as_posix() for p in (target.rglob("*.cs") if target.is_dir() else [target]))
    for prefix in prefixes:
        matches = [p for p in names if p.endswith(".cs") and (p == prefix or p.startswith(prefix + "/"))]
        if not matches:
            raise ValueError(f"保护范围无源码：{prefix}")
        for path in matches:
            content = git("show", f"{revision}:{path}") if revision else (ROOT / path).read_bytes()
            # Git/Windows 检出换行不属于机制差异；严格 UTF-8 解码后统一换行比较。
            normalized = content.decode("utf-8-sig").replace("\r\n", "\n").encode("utf-8")
            files[path] = hashlib.sha256(normalized).hexdigest()
    return {"schemaVersion": 1, "revision": revision or "工作区", "protectedPaths": sorted(prefixes), "files": dict(sorted(files.items()))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--snapshot", type=Path, help="写入快照 JSON（不要放入保护源码目录）")
    group.add_argument("--compare", type=Path, help="将当前工作区与已有快照比较")
    parser.add_argument("--revision", help="快照使用指定 Git 提交；省略则读取工作区")
    args = parser.parse_args()
    if args.compare and args.revision:
        parser.error("--revision 仅用于 --snapshot")
    current = snapshot(args.revision)
    if args.snapshot:
        output = args.snapshot.resolve()
        for prefix in current["protectedPaths"]:
            target = (ROOT / prefix).resolve()
            if output == target or output.is_relative_to(target):
                raise ValueError("快照不能写入保护范围")
        if output == MANIFEST:
            raise ValueError("快照不能覆盖清单")
        output.write_text(json.dumps(current, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"已记录 {len(current['files'])} 个源码文件。")
        return 0
    before = json.loads(args.compare.read_text(encoding="utf-8-sig"))
    if before["protectedPaths"] != current["protectedPaths"]:
        raise ValueError("保护清单发生变化，须先核对范围并重新建基线")
    changed = sorted(p for p in before["files"].keys() | current["files"].keys() if before["files"].get(p) != current["files"].get(p))
    if changed:
        print("以下源码发生变更，须审查是否为已授权的 API 适配或机制更新：")
        print("\n".join(changed))
        return 1
    print(f"通过：{len(current['files'])} 个保护源码文件内容一致。")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (ValueError, OSError, subprocess.CalledProcessError) as ex:
        print(f"检查失败：{ex}", file=sys.stderr)
        sys.exit(2)
