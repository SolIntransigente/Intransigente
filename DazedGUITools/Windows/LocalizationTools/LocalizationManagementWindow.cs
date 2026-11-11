using System;
using System.IO;
using Dazed.Vitals;
using Dazed.Vitals.Localization;
using ImGuiNET;
using Newtonsoft.Json;

namespace DazedGUITools.Windows.LocalizationTools;

[MainMenuWindow("Localization Manager")]
public class LocalizationManagementWindow : WindowBaseClass
{
	private bool _creatingLocal;
	private static bool CanSave => File.Exists(LocalizationData.MainStringTablePath) && File.Exists(LocalizationData.CurrentStringTablePath);
	
	public LocalizationManagementWindow() : base("Localization Manager", true)
	{
        LocalizationData.LoadLocalization();
    }

    protected override void Draw()
	{
		ImGui.Text("Localization Manager");
		ImGui.Spacing();

		if(IsDirty) 
			ImGui.TextColored(Vector4Colors.Red, "THERE ARE UNSAVED CHANGES!");
		ImGui.Separator();
		if (ImGui.Button("Close"))
		{
			Close();
		}

		if (CanSave && ImGui.Button("Save"))
		{
			LocalizationData.SaveTable();
			IsDirty = false;
		}

        ImGui.Separator();

		if (!LocalizationData.MainStringTableAssigned)
		{
			ShowTableCreation();
			return;
		}
		if(LocalizationData.LocalizationsExist && ImGui.Button(!_creatingLocal ? "Create new local" : "Cancel local creation"))
			_creatingLocal = !_creatingLocal;
		if (!LocalizationData.LocalizationsExist || _creatingLocal)
		{
			ImGui.Spacing();
			ImGui.TextColored(Vector4Colors.Green, "Local Creation");
			ImGui.Text(_creatingLocal ? "Enter the proper values for the local." : "No Localizations found.");
			ImGui.Spacing();
			
			CreateLocalization();
			if (!LocalizationData.LocalizationsExist)
				return;
			ImGui.Spacing();
			ImGui.Spacing();
		}
		ImGui.Separator();
		
		ImGui.Text("Current Local Data");
		
		var index = LocalizationData.CurrentLanguageIndex;
		if (ImGui.Combo("Localizations", ref index, LocalizationData.AvailableLocalizations, LocalizationData.LocalizationPath.Count) && LocalizationData.CurrentLanguageIndex != index)
		{
            LocalizationData.CurrentLanguageIndex = index;
			LocalizationData.LoadLocalization();
		}

		if (ImGui.Button("Add Item"))
		{
			LocalizationData.AddIdentifier(LocalizationData.ItemsCount.ToString());
			LocalizationData.AddEntry("none");
			IsDirty = true;
		}

		ImGui.Spacing();
		ImGui.Spacing();
		if (LocalizationData.ItemsCount == 0)
			ImGui.Text("No localization entry exists!");
		
		else if (ImGui.BeginTable("local", 2, ImGuiTableFlags.None))
		{
			ImGui.TableSetupColumn("Identifier");
			ImGui.TableSetupColumn("Value");
			ImGui.TableHeadersRow();
			for (var i = 0; i < LocalizationData.ItemsCount; i++)
				DrawRow(i);
			ImGui.EndTable();
            LocalizationData.DeleteQueuedItem();
		}
	}

	private void DrawRow(int i)
	{
		ImGui.TableNextColumn();
		var identifier = LocalizationData.GetIdentifier(i);
		if (ImGui.InputText($"Identifier##{i}", ref identifier, 128))
		{
			LocalizationData.SetIdentifierValue(i, identifier);
			IsDirty = true;
		}
		ImGui.TableNextColumn();
        var value = LocalizationData.GetEntry(i);
		if (ImGui.InputText($"Value##{i}", ref value, (int)short.MaxValue, ImGuiInputTextFlags.CtrlEnterForNewLine))
		{
			LocalizationData.SetEntryValue(i, value);
			IsDirty = true;
		}
		ImGui.SameLine();
		if(ImGui.SmallButton($"Remove item##{i}"))
		{
            LocalizationData.QueueItemDeletion(i);
			IsDirty = true;
		}
	}

	private string _localIdentifier = "";
	private string _localFormalName = "";
	
	private void CreateLocalization()
	{
		ImGui.InputText("Local identification", ref _localIdentifier, 128);
		ImGui.InputText("Local formal name", ref _localFormalName, 128);
		var filePath = Path.Combine(LocalizationData.LocalizationFolderPath, $"{_localIdentifier}{Locals.LocalInfoExtension}");
		var stringTablePath = Path.Combine(LocalizationData.LocalizationFolderPath, $"{_localIdentifier}{Locals.StringTableFileExtension}");

		var canCreate = PathUtils.IsValidPath(filePath) && PathUtils.IsValidPath(stringTablePath) && !string.IsNullOrWhiteSpace(_localFormalName) && !string.IsNullOrWhiteSpace(_localIdentifier);
		if(!canCreate)
			ImGui.TextColored(Vector4Colors.Red, "Please enter valid values");
		if (ImGui.Button("Create Localization Table") && canCreate)
		{
			filePath = filePath.Trim();
			stringTablePath = stringTablePath.Trim();
			using var f = File.Create(filePath);
			f.Dispose();
			using var f2 = File.Create(stringTablePath);
			f2.Dispose();
			File.WriteAllText(filePath,
				JsonConvert.SerializeObject(new SerializedLanguageInfo(_localIdentifier, _localFormalName, Path.GetFileName(stringTablePath))));
			File.WriteAllText(stringTablePath, JsonConvert.SerializeObject(new SerializedStringTable(Array.Empty<string>())));
			LocalizationData.FindLocalizations();
			LocalizationData.LoadLocalization();
			_localIdentifier = "";
			_localFormalName = "";
		}
	}

	private void ShowTableCreation()
	{
		if (ImGui.Button("Create Localization Table"))
		{
			if(!Directory.Exists(LocalizationData.LocalizationFolderPath))
				Directory.CreateDirectory(LocalizationData.LocalizationFolderPath);
			using var f = File.Create(LocalizationData.MainStringTablePath);
			LocalizationData.CreateEmptyMainStringTable();
        }
	}
}