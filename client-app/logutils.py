import sys


def debug(msg):
    print(msg)


def warn(msg):
    print(msg)


def error(msg):
    print(msg, file=sys.stderr)
