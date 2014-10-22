#! /bin/bash

# This script fixes an error with the ENet library occuring on some Linux systems.
# Filed as bug here: https://github.com/manicdigger/manicdigger/issues/124

# Unhandled Exception:
# ENet.ENetException: The ENet native library failed to initialize.
#    Make sure ENetX86.dll and ENetX64.dll are in the program directory, and that you are running on a x86 or x64-based computer.
#    If you are running on Linux, make sure the libenet.so.1 is in your path.
#    On Ubuntu Linux, install the libenet1a package (1.3.3 or newer) if you haven't already.
#    If you are running on MacOS, make sure libenet.dylib is in your path or program directory.

sudo ln -s /usr/lib/libenet.so /usr/lib/libenet.so1
