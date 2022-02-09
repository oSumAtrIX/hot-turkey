using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Cocona;
using CtActivator;

CoconaApp.Run(([Argument(Description = "Cold Turkey Blocker.exe")][FileExists]string file) =>
{
	ModuleDefinition? module = null;
	
	try
	{
		module = ModuleDefinition.FromFile(file);
	}
	catch (BadImageFormatException)
	{
		Console.WriteLine("Input file does not contain a .NET module.");
		return;
	}

	if (module?.Name?.Contains("Cold Turkey Blocker") is bool containsName && containsName)
	{	
		Console.WriteLine("This is not Cold Turkey Blocker.exe. Aborting.");
		return;
	}

	TypeDefinition? additionalsType = module?.GetAllTypes()
		.SingleOrDefault(t => t?.Name == "Additionals");

	if (additionalsType is null)
	{
		Console.WriteLine("Unable to get the type definition which needs to be patched.");
		return;
	}

	MethodDefinition? proStatusGetterMethod = additionalsType
		.Methods
		.SingleOrDefault(m => m.Name == "get_ProStatus");

	if (proStatusGetterMethod is null)
	{
		Console.WriteLine("The method which checks for pro status could not be found.");
		return;
	}
	
	CilInstructionCollection instructions = proStatusGetterMethod.CilMethodBody?.Instructions!;

	if (instructions[0].OpCode.Code == CilCode.Ldstr)
	{
		Console.WriteLine("File already patched. Aborting.");
		return;
	}
	
	instructions.RemoveAt(0);
	instructions[0] = new CilInstruction(CilOpCodes.Ldstr, "pro");

	try
	{
		File.Move(file , file + ".bak");
	}
	catch (Exception)
	{
		Console.WriteLine("The original file could not be backed up. Back it up manually and proceed by pressing the enter key.");
		Console.ReadLine(); 
	}
	
	try
	{
		module?.Write(file);
	}
	catch (AggregateException)
	{
		Console.WriteLine("The file was patched successfully but could not be constructed.");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Failed to save the file to disk. Try running with administrative permissions.\nDetails: {ex.Message}");
	}
});