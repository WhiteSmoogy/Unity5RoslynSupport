# ����빹��
����Ŀͨ���ṩαװ��������ʵ�ֵ�������������ض���,�Դﵽ�滻��ʹ�ñ�������Ŀ�ģ�����������֮���ܻ������ô���
>1. �������߱������ٶ�(�������ݼ���)
>2. ʹ������C#�﷨

## 1.Roslyn
����Ŀ������Rolsyn��Ŀ commit��¼��Ӧ2017������һ��[release tag](https://github.com/dotnet/roslyn/releases/tag/version-2.6.0-beta3 "version-2.6.0-beta3")
����ĿҪ���ȹ���Roslyn,���ڼ򵥷����Ŀ��,ʹ�����²��蹹��Roslyn
>1. ��װ����[.Net Core SDK](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.3-windows-x64-installer)
>2. ��¡��ʼ��Roslyn��submodule,�����Ŀ¼ִ��������������:
>3. dotnet restore build\ToolsetPackages\BaseToolset.csproj
>4. dotnet restore Compiler.sln

## 2.mcs
mcs��ָ�����������ʵ��,smcsֱ���ض���ĳ��Ŀ¼��mcsִ�г��򡣱���Ŀ�е�mcs��Ҫ��ɵ���Log���������������������mdb���,���ﵽ����Unity��Ҫ�ı��������,�ṩUnity��������Ҫ����Ϣ
��Roslyn������,�޸�mcs.csprojΪ����:
```
<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>netcoreapp2.0</TargetFramework>
		<RuntimeFrameworkVersion>2.0.4</RuntimeFrameworkVersion>
</PropertyGroup>
```
��mcs.projĿ¼��ִ������ 
>dontet publish -c Release -r win7-x64 -f netcoreapp2.0 -o Roslyn
	
�������RoslynĿ¼������ %UnityĿ¼%\Editor\Data\Mono\lib\mono
