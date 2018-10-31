# dotnet-install (dni)

This is a .NET Core Global tool that primarily is aimed at Linux it will work on other operating system but YMMV.  We would welcome any PRs to make cross platform installs more reliable!

This tool enables the installation of side by side(SxS) .NET Core SDKs through an easier mechanism.  Currently the .NET Core Linux installation instructions do not mention side by side and 
any attempt to do SxS fails as the latest version is always installed.

### Commands

To get an understanding of the commands you can execute run `dni --help`