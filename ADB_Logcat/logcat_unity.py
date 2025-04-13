#!/usr/bin/python
#
# Usage:
# adb logcat -v time | python logcat_unity.py

import sys
import re

IGNORED_LINES=[
    "(Filename:",
    "libunity.so",
    "libil2cpp.so",
    "boot.oat",
    "StackTrace:",
    "at UnityEngine.",
    "#"
]
# Show logs from these tags
INCLUDED_TAGS = ["Unity", "VLCRadioService"]

REGEX_LINE = r"([0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]{3}) ([DIWEF])\/(\w+)\s*\(\s*\d+\):(.*)"

RED="\033[1;31m"
YELLOW="\033[1;33m"
NO_COLOR="\033[0m"

def colorize(color, text):
    return color + text + NO_COLOR

def isDebugOrInfo(log_level):
    log_level = log_level.rstrip()
    return log_level=="I" or log_level=="D"

def isException(log_message):
    containsException = (log_message.find("Error") != -1) or (log_message.find("Exception") != -1)
    return containsException or log_message.strip().startswith("at ")

def isIgnored(log_message):
    for ignored in IGNORED_LINES:
        if ignored in log_message:
            return True
    return False


while True:
    line = sys.stdin.readline().rstrip()
    match = re.search(REGEX_LINE, line)

    if not match:
        continue

    time = match.group(1)
    log_level = match.group(2).strip()
    tag = match.group(3)
    log_message = match.group(4)

    if not log_message.strip():
        continue

    if tag not in INCLUDED_TAGS:
        continue
    
    if isIgnored(log_message):
        continue

    output = f"{time} {log_level} [{tag}] {log_message}"

    if isException(log_message):
        print(colorize(RED, output))	
    elif not isDebugOrInfo(log_level):
        print(colorize(YELLOW, output))
    else:
        print (output)