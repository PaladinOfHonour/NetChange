@echo off
cd obj/x86/Debug
start MultiClientServer 1100 1101
start MultiClientServer 1101 1100
