#!/usr/bin/env python3
"""AMEN.D visual-analysis library assembled from reviewable source parts."""
from pathlib import Path as _Path

_globals = globals()
for _name in (
    "AmenDReference.Part1.py",
    "AmenDReference.Part2.py",
    "AmenDReference.Part3.py",
    "AmenDReference.Part4.py",
):
    _path = _Path(__file__).with_name(_name)
    exec(compile(_path.read_text(encoding="utf-8"), str(_path), "exec"), _globals, _globals)
del _Path, _globals, _name, _path
