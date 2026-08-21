# Magic Excel

Magic Excel imports Google Sheets into Unity, generates strongly typed C# classes, and writes the data into a generated `ExcelData.asset` for runtime use.

## What it generates

For regular sheets, Magic Excel generates:

- one serializable C# class per sheet
- one `ExcelData` ScriptableObject with one array per generated class

For sheets whose names end with `Setting`, Magic Excel generates:

- one static C# class with constant values

## Installation

Install with Unity Package Manager using the Git URL below:

```text
https://github.com/phamkhacquang/MagicExcel.git
```

Open the tool from:

```text
Window > Magic Excel
```

## Google Sheets example

Example spreadsheet:

https://docs.google.com/spreadsheets/d/12-r4UWAkMeNAR6NZsWkf-fv8Bxcj9avIIN2grKVfzyY

When configuring Magic Excel, use the spreadsheet ID part:

```text
12-r4UWAkMeNAR6NZsWkf-fv8Bxcj9avIIN2grKVfzyY
```
## Quick setup

Magic Excel downloads each Google Sheet as an `.xlsx` file and reads it with ExcelDataReader.

In Unity:

1. Open `Window > Magic Excel`.
2. Fill in **Spreadsheet Ids**.
3. Set **Script Namespace**.
4. Set **Output Folder** if needed.
5. Optionally configure **Additional Namespaces**, **Class Name Format**, or **Sheet Custom Mappings**.
6. Click `Output`.

Default output paths:

- `Assets/ExcelData/Scripts`
- `Assets/ExcelData/Assets/ExcelData.asset`

If generated scripts change, Unity recompiles first and Magic Excel serializes the asset automatically after reload.

## Spreadsheet rules

### Ignore sheets

Any sheet whose name starts with `Ignore` is skipped.

Examples:

- `IgnoreTemp`
- `Ignore_Notes`

### Setting sheets

Any sheet whose name ends with `Setting` is treated as a settings sheet.

- it generates a static class instead of runtime data in `ExcelData`
- the first row defines fields
- the second row provides the values
- array fields become `static readonly`

Example:

```text
int MaxLevel | float DropRate | string WelcomeMessage
99           | 0.35          | Hello
```

Generates:

```csharp
public static class GameSetting
{
	public const int MaxLevel = 99;
	public const float DropRate = 0.35f;
	public const string WelcomeMessage = "Hello";
}
```

### Header format

The first row of each normal sheet defines fields:

```text
type fieldName
```

Examples:

```text
int ID
Name //If the type is omitted, Magic Excel treats the field as `string`
float Price
bool IsActive
int[] RewardIds
```

## Supported values

Magic Excel supports:

- `string`
- `bool`
- numeric primitive types such as `int`, `float`, `double`, `long`, `short`, and `byte`
- enums
- arrays such as `int[]`, `float[]`, `string[]`, and enum arrays
- types supported by .NET string conversion
- custom types with `public static Parse(string)`

Notes:

- `bool` also accepts integers: `0` is `false`, any non-zero value is `true`
- enums are parsed by name, case-insensitively
- arrays are separated by comma, semicolon, or newlines inside one cell
- if multiple spreadsheets contain the same sheet name, only the first one is used

## Runtime usage

You must call `Init()` on the generated `ExcelData` asset before reading static data.

```csharp
using Excel;
using MagicExcel;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
	[SerializeField] private ExcelData excelData;

	private void Awake()
	{
		excelData.Init();
	}
}
```

After that, access data through the generated static arrays:

```csharp
var firstProduct = ExcelData.Product[0];
Debug.Log(firstProduct.Name);

var product = ExcelData.Product.GetBy(x => x.ID == 2);
Debug.Log(product?.Name);

Debug.Log(GameSetting.MaxLevel);
```