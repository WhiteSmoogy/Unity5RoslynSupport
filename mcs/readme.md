# mcs介绍
该项目为Roslyn编译器的包装,直接调用csc中的Main函数,在编译完成后,调用pdb2mdb.exe转换调试信息格式。

Unity会把自己的编译命令写入工程目录的临时文件中,所以第一个参数其实是文件,里面包含了Unity要求的编译命令,除去这些命令，我们还需要增加以下命令来达到兼容性目的:
1.	-nostdlib+ 引用指定目录的mscorlib.dll
2.  -noconfig 不使用自带的rsp配置
3.	-nologo 不输出版本信息,Unity输出兼容

该项目引用的dll默认选取Program Files(X86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v3.5/Profile中的dll 这样与vs打开Unity的csproj中引用路径保持一致性，并且当前发起的smcs路径来判别是Subset还是Full

关于语言版本，Unity默认支持的版本为5，当前Roslyn分支的语言版本为7.2