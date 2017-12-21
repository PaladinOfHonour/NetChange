@echo off
cd obj/x86/Debug
start MultiClientServer 1100 1102 1101
start MultiClientServer 1101 1100 1102
start MultiClientServer 1102 1100 1101 1106 1104
start MultiClientServer 1104 1102 1105
start MultiClientServer 1105 1104 1106
start MultiClientServer 1106 1105 1102