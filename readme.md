# 简介与构建
本项目通过提供伪装编译器来实现到具体编译器的重定向,以达到替换所使用编译器的目的，升级编译器之后能获得两点好处：
1. 大幅度提高编译器速度
2. 使用最新C#语法

## 1.Roslyn
本项目依赖的Roslyn项目 commit记录对应2017年的最后一次[release tag](https://github.com/dotnet/roslyn/releases/tag/version-2.6.0-beta3 "version-2.6.0-beta3")
本项目要求先构建Roslyn,处于简单方便的目的,使用以下步骤构建Roslyn
1. 安装最新[.Net Core SDK](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.3-windows-x64-installer)
2. 克隆初始化Roslyn的submodule,进入该目录执行以下两条命令:
3. dotnet restore build\ToolsetPackages\BaseToolset.csproj
4. dotnet restore Compiler.sln

## 2.mcs
mcs将指代多个编译器实现,smcs直接重定向到某个目录下mcs执行程序。本项目中的mcs主要完成的是Log输出、编译器命令修正、mdb输出,来达到兼容Unity想要的编译器输出,提供Unity调试所需要的信息
在Roslyn构建后,修改mcs.csproj为如下:
```
<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>netcoreapp2.0</TargetFramework>
		<RuntimeFrameworkVersion>2.0.4</RuntimeFrameworkVersion>
</PropertyGroup>
```
在mcs.proj目录下执行命令 
dotnet publish -c Release -r win7-x64 -f netcoreapp2.0 -o Roslyn

将输出的Roslyn目录拷贝至 %Unity目录%\Editor\Data\Mono\lib\mono

## 3.smcs
smcs为伪装的编译器,注意该程序集会是mono加载执行，所以只做具体编译器的程序启动,将编译输出的smcs.exe进行如下操作:
1. 重命名%Unity目录%\Editor\Data\Mono\lib\mono\2.0\gmcs.exe 为mcs.exe,拷贝smcs.exe至该目录。
2. 重命名%Unity目录%\Editor\Data\Mono\lib\mono\Unity\smcs.exe 为mcs.exe,拷贝smcs.exe至该目录。

# 行为配置
程序优先使用Unity工程目录下的Roslyn.json文件，如果不存在则使用Roslyn目录下的config.json,行为配置支持或将要支持以下行为
1. UseRoslyn:bool 控制是否使用Roslyn编译器
2. LanguageVersion:string 控制使用的C#语言版本
	* Unity 默认行为,当前Unity最大支持的版本
    * Number 6,7,7.2 数字，指定具体版本号
    * Laster 当前Roslyn支持的最大版本
3. DynamicSupport:bool 是否引用dynamic所需要的Microsoft.Csharp库
4. ReferenceDir:string 控制编译器时所使用的系统库目录
	* %.NetFx% 默认,使用Program File下的dll
    * %.Mono%  使用当前smcs.exe目录下的dll
    * %.Roslyn% 使用Roslyn目录下的dll
    * 绝对路径 使用该目录下的dll
5. OutputLog:bool 控制是否输出log文件
