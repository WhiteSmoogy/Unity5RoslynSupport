# ����빹��
����Ŀͨ���ṩαװ��������ʵ�ֵ�������������ض���,�Դﵽ�滻��ʹ�ñ�������Ŀ�ģ�����������֮���ܻ������ô���
1. �������߱������ٶ�
2. ʹ������C#�﷨

## 1.Roslyn
����Ŀ������Roslyn��Ŀ commit��¼��Ӧ2017������һ��[release tag](https://github.com/dotnet/roslyn/releases/tag/version-2.6.0-beta3 "version-2.6.0-beta3")
����ĿҪ���ȹ���Roslyn,���ڼ򵥷����Ŀ��,ʹ�����²��蹹��Roslyn
1. ��װ����[.Net Core SDK](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.3-windows-x64-installer)
2. ��¡��ʼ��Roslyn��submodule,�����Ŀ¼ִ��������������:
3. dotnet restore build\ToolsetPackages\BaseToolset.csproj
4. dotnet restore Compiler.sln

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
dotnet publish -c Release -r win7-x64 -f netcoreapp2.0 -o Roslyn

�������RoslynĿ¼������ %UnityĿ¼%\Editor\Data\Mono\lib\mono

## 3.smcs
smcsΪαװ�ı�����,ע��ó��򼯻���mono����ִ�У�����ֻ������������ĳ�������,�����������smcs.exe�������²���:
1. ������%UnityĿ¼%\Editor\Data\Mono\lib\mono\2.0\gmcs.exe Ϊmcs.exe,����smcs.exe����Ŀ¼��
2. ������%UnityĿ¼%\Editor\Data\Mono\lib\mono\Unity\smcs.exe Ϊmcs.exe,����smcs.exe����Ŀ¼��

# ��Ϊ����
��������ʹ��Unity����Ŀ¼�µ�Roslyn.json�ļ��������������ʹ��RoslynĿ¼�µ�config.json,��Ϊ����֧�ֻ�Ҫ֧��������Ϊ
1. UseRoslyn:bool �����Ƿ�ʹ��Roslyn������
2. LanguageVersion:string ����ʹ�õ�C#���԰汾
	* Unity Ĭ����Ϊ,��ǰUnity���֧�ֵİ汾
    * Number 6,7,7.2 ���֣�ָ������汾��
    * Laster ��ǰRoslyn֧�ֵ����汾
3. DynamicSupport:bool �Ƿ�����dynamic����Ҫ��Microsoft.Csharp��
4. ReferenceDir:string ���Ʊ�����ʱ��ʹ�õ�ϵͳ��Ŀ¼
	* %.NetFx% Ĭ��,ʹ��Program File�µ�dll
    * %.Mono%  ʹ�õ�ǰsmcs.exeĿ¼�µ�dll
    * %.Roslyn% ʹ��RoslynĿ¼�µ�dll
    * ����·�� ʹ�ø�Ŀ¼�µ�dll
5. OutputLog:bool �����Ƿ����log�ļ�
