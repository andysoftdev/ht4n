HOW TO BUILD ht4n
=================

Building ht4n from source requires at least Microsoft Visual Studio 2010 Professional.


###Browse through or download the ht4n source###

* Browse through or download the ht4n source from [github](http://github.com/andysoftdev/ht4n).
  To download the latest sources press the 'Downloads' button and choose one of the following files, either the
  Download.tar.gz or the Download.zip.
  
* Alternatively, get the source from the repository, create a projects folder (the path must not contain any spaces) and use:

		mkdir hypertable
		cd hypertable
		git clone git://github.com/andysoftdev/ht4c.git
		git clone git://github.com/andysoftdev/ht4n.git


###Download and build Hypertable for Windows###

* See [How To build Hypertable for Windows](https://github.com/andysoftdev/ht4w/blob/windows/README.md).


###Build ht4n###

* In order to build the ht4n API documentation download and install [Sandcastle](http://sandcastle.codeplex.com/) and the [Sandcastle Help File Builder](http://shfb.codeplex.com/).

* In order to run the ht4n test suite download and install the [MSBuild Community Tasks](http://msbuildtasks.tigris.org/). Notice the ht4n x64 test suite requires a mstest64.exe:

		cd %VSInstallDir%\Common7\IDE
		copy mstest.exe mstest64.exe
		copy mstest.exe.config mstest64.exe.config
		corflags mstest64.exe /32BIT- /Force
  and copy the complete registry key

		HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\EnterpriseTools\QualityTools\TestTypes
  to

		HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\10.0\EnterpriseTools\QualityTools\TestTypes

* Open the ht4n solution (ht4n\ht4n.sln) with Microsoft Visual Studio 2010 and build its configurations. Alternatively, open the Visual Studio command prompt and type:

		cd ht4n
		msbuild ht4n.buildproj
  or, for a complete rebuild, type

		cd ht4n
		msbuild ht4n.buildproj /t:Clean;Make

