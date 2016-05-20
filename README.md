# zCrypt

zCrypt is a utility program which provides straight-forward, powerful functions for protect, secure, crypt or split files and folders.

zCrypt is lightweigth, portable, and very easy to use.

zCrypt is compiled with Dotnet Core framework. That's why it works on various platforms (Windows, Linux and Mac).

zCrypt is developped by http://www.zem.fr team and it's absolutely free for use.

## zCrypt Goal

zCrypt was developed because we need to secure data before synchronize to cloud storage like Amazon Cloud Drive or Hubic. These cloud storage providers don't encrypt uploaded files.

After looking for a solution, we don't find free and good program to protect our files.

That's why we decide to develop our own program: zCrypt.


## zCrypt Functionalities

### Function Crypt

The crypt function permits to protect a file or a folder using AES encryption algorythm. You only need to provide a secure password (at least 8 caracters) to encrypt data.

```bash
zCrypt.exe crypt "encryptionPassword" "sourceDirOrFile" "outputPath"
```

The result of crypt command is a crypted file, named zCryptFile, unreadable without zCrypt decrypt command. The zCryptFile use .zc file extension.

### Function Derypt

The decrypt function permits to restore any zCryptFile protected content. You only need to provide the password used to encrypt data. Without the good password, it's impossible to uncrypt the file.

```bash
zCrypt.exe decrypt "encryptionPassword" "cryptedFilePath" "outputPath"
```

### Function Split

The split function permits to split large file into small junk files. You can specify the size of junk files. This function is very useful when you need to upload a big file: split it and upload smaller files :-)

```bash
zCrypt.exe split sizeInMb "sourceFile" "outputPath"
```

Junk files ends with *_xxxx (x is a number between 0 and 9). The first junk is *_0000.

### Function Assemble

The assemble function permits to assemble junk files into a single file (like the original source file used with split command).

```bash
zCrypt.exe assemble "splitFile" "outputPath"
```

Parameter "splitFile" have to be the first junk file (*_0000).

### Function List

The list function displays the content of a zCryptFile. It lists directories and files into a crypted file.

```bash
zCrypt.exe list "encryptionPassword" "cryptedFilePath" ["outputFile"]
```

### Function ListAll

The list function displays the content of all zCryptFile (*.zc and *.zc_0000) founded into a directory.

```bash
zCrypt.exe listall "encryptionPassword" "sourceDirOrFile" ["outputFile"]
```

## zCrypt Help

zCrypt application displays this help:

```bash
=========================================================================
= zCrypt, secure your data quickly !!! www.zem.fr                       =
=========================================================================
zCrypt allows you to encrypt files and directories into a crypted single file
It uses AES encryption to protect your files

Syntax to crypt:
zCrypt.exe crypt "encryptionPassword" "sourceDirOrFile" "outputPath"

Syntax to decrypt:
zCrypt.exe decrypt "encryptionPassword" "cryptedFilePath" "outputPath"

Syntax to list directories and files into a crypted file:
zCrypt.exe list "encryptionPassword" "cryptedFilePath" ["outputFile"]

Syntax to list directories and files into all crypted files (*.zc and *.zc_0000):
zCrypt.exe listall "encryptionPassword" "sourceDirOrFile" ["outputFile"]

Syntax to split a file into junkfiles:
zCrypt.exe split sizeInMb "sourceFile" "outputPath"

Syntax to assemble junkfiles into a single file:
zCrypt.exe assemble "splitFile" "outputPath"

Parameters:
- sourceDirOrFile: source directory or file to encrypt or split
- outputPath: output directory to store generated files (crypted, decrypted, splitted or assemble)
- outputPath: output file to store command result
- cryptedFilePath: path to the file you want to decrypt
- encryptionPassword: password used to protect your files (at least 8 chars)
=========================================================================
= Process finished in 00:00:00.0320014. Press Enter to exit.            =
=========================================================================
```

## zCrypt Compilation

zCrypt is totaly free and it's source code is available on GitHub.

zCrypt is developed in C# using Dotnet Core framework. We select core framework because it is portable on multi distributions and operating systems.

Moreover C# is a very readable language which allows maintenances and it permits anyone to read the source code easily.

zCrypt is developed under Visual Studio 2015 Update 2, but project solution could be compiled in command line. You could refer to Dotnet core documentation to learn more about building .Net core projets.

## zCrypt Tests

zCrypt is tested using various environments:

* Windows 7 x86
* Windows 7 x64
* Windows 10 x64
* Ubuntu Server LTS x86
* Ubuntu Server LTS x64

Each zCrypt release and update is tested on these platforms.

## Issues and Contact

If you encountered problems or difficulties installing, compiling or using zCrypt, you can open a GitHub issue.

You can also visit our blog at http://www.zem.fr


































































































































































