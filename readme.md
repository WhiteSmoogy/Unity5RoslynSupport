# 简介与构建
本项目通过提供伪装编译器来实现到具体编译器的重定向,以达到替换所使用编译器的目的，升级编译器之后能获得两点好处：
>1. 大幅度提高编译器速度(具体数据见下)
>2. 使用最新C#语法

## 1.Roslyn
本项目依赖的Rolsyn项目 commit记录对应2017年的最后一次[release tag](https://github.com/dotnet/roslyn/releases/tag/version-2.6.0-beta3 "version-2.6.0-beta3")
本项目要求先构建Roslyn,处于简单方便的目的,使用以下步骤构建Roslyn
>1. 安装最新[.Net Core SDK](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.3-windows-x64-installer)
>2. 克隆初始化Roslyn的submodule,进入该目录执行以下两条命令:
>3. dotnet restore build\ToolsetPackages\BaseToolset.csproj
>4. dotnet restore Compiler.sln

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
>dontet publish -c Release -r win7-x64 -f netcoreapp2.0 -o Roslyn
	
将输出的Roslyn目录拷贝至 %Unity目录%\Editor\Data\Mono\lib\mono
