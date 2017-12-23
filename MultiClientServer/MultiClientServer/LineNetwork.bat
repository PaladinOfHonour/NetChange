@echo off
cd obj/x86/Debug
start MultiClientServer 1101 1102
start MultiClientServer 1102 1103 1101
start MultiClientServer 1103 1104 1102
start MultiClientServer 1104 1105 1103
start MultiClientServer 1105 1106 1104
start MultiClientServer 1106 1107 1105