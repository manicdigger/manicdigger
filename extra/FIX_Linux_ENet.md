# Workaround for ENet errors on Linux

This is a workaround for an error with the ENet library occuring on some Linux systems.  
Filed as a bug here: https://github.com/manicdigger/manicdigger/issues/124 (solved)

```
Unhandled Exception:
ENet.ENetException: The ENet native library failed to initialize.
	Make sure ENetX86.dll and ENetX64.dll are in the program directory, and that you are running on a x86 or x64-based computer.
	If you are running on Linux, make sure the libenet.so.7 is in your path.
	On Ubuntu/Debian Linux, install the libenet7 package if you haven't already.
	If you are running on MacOS, make sure libenet.dylib is in your path or program directory.
```

This is caused by different package versions of the networking library Manic Digger uses.  
In general you need to **create a symlink** to redirect requests to the appropiate library if you get the issue:

Below are instructions for the most common Linux variants.  

## Debian
#### 7 (Wheezy)
Only **libenet1a** is available in the official package repositories.  
You can fix the issue by creating a symlink to `libenet.so.1`
###### 32-bit
```bash
ln -s /usr/lib/i386-linux-gnu/libenet.so.1 /usr/lib/i386-linux-gnu/libenet.so.7
```
###### 64-bit
```bash
ln -s /usr/lib/x86_64-linux-gnu/libenet.so.1 /usr/lib/x86_64-linux-gnu/libenet.so.7
```
#### 8 (Jessie)
**libenet7** is included in the official package repositories. You can install it from there.
```bash
apt-get install libenet7
```

## Ubuntu
#### Precise
Only **libenet1a** is available in the official package repositories.  
You can fix the issue by creating a symlink to `libenet.so.1`
###### 32-bit
```bash
ln -s /usr/lib/i386-linux-gnu/libenet.so.1 /usr/lib/i386-linux-gnu/libenet.so.7
```
###### 64-bit
```bash
ln -s /usr/lib/x86_64-linux-gnu/libenet.so.1 /usr/lib/x86_64-linux-gnu/libenet.so.7
```
#### Trusty
Only **libenet2a** is available in the official package repositories.  
You can fix the issue by creating a symlink to `libenet.so.2`
###### 32-bit
```bash
ln -s /usr/lib/i386-linux-gnu/libenet.so.2 /usr/lib/i386-linux-gnu/libenet.so.7
```
###### 64-bit
```bash
ln -s /usr/lib/x86_64-linux-gnu/libenet.so.2 /usr/lib/x86_64-linux-gnu/libenet.so.7
```
#### Utopic, Vivid
**libenet7** is included in the official package repositories. You can install it from there.
```bash
apt-get install libenet7
```

## Mint 17.1 (64-bit)
Only **libenet2a** is available in the official package repositories.  
You can fix the issue by creating a symlink to `libenet.so.2`
```bash
sudo ln -s /usr/lib/x86_64-linux-gnu/libenet.so.2 /usr/lib/x86_64-linux-gnu/libenet.so.7
```
