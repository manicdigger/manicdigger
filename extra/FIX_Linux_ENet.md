Workaround for ENet errors on Linux
===================================

This is a workaround for an error with the ENet library occuring on some Linux systems.
Filed as a bug here: https://github.com/manicdigger/manicdigger/issues/124

```
Unhandled Exception:
ENet.ENetException: The ENet native library failed to initialize.
	Make sure ENetX86.dll and ENetX64.dll are in the program directory, and that you are running on a x86 or x64-based computer.
	If you are running on Linux, make sure the libenet.so.1 is in your path.
	On Ubuntu Linux, install the libenet1a package (1.3.3 or newer) if you haven't already.
	If you are running on MacOS, make sure libenet.dylib is in your path or program directory.
```

In general you need to **create a symlink** to redirect requests to the no longer existing library:

This could look like the following.  
Please note that the path to your libenet.so.xxx file may differ. See Mint section below for an example.
```
sudo ln -s /usr/lib/libenet.so /usr/lib/libenet.so.1
```

Linux Mint 17.1
---------------

There's only the package libenet2a available.
You can use the following command here to make things work:
```
sudo ln -s /usr/lib/x86_64-linux-gnu/libenet.so.2 /usr/lib/libenet.so.1
```
