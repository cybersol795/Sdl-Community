﻿using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Sdl.Community.projectAnonymizer.Helpers;
using Sdl.Community.projectAnonymizer.Models;
using Sdl.Community.projectAnonymizer.Process_Xliff;
using Sdl.Community.projectAnonymizer.Ui;
using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.FileTypeSupport.Framework.Core.Utilities.BilingualApi;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectAutomation.AutomaticTasks;
using Sdl.ProjectAutomation.Core;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using Sdl.TranslationStudioAutomation.IntegrationApi.Presentation.DefaultLocations;
using Constants = Sdl.Community.projectAnonymizer.Helpers.Constants;

namespace Sdl.Community.projectAnonymizer.Batch_Task
{
	[ApplicationInitializer]
	public class ApplicationInitializer : IApplicationInitializer
	{
		public void Execute()
		{
			CreateAcceptFile();
			var acceptWindow = new AcceptWindow();
			if (!AgreementMethods.UserAgreed())
			{
				acceptWindow.ShowDialog();
			}
		}
		private void CreateAcceptFile()
		{
			if (!Directory.Exists(Constants.AcceptFolderPath))
			{
				Directory.CreateDirectory(Constants.AcceptFolderPath);
			}

			if (File.Exists(Constants.AcceptFilePath)) return;
			var file =File.Create(Constants.AcceptFilePath);
			file.Close();
			var accept = new Agreement
			{
				Accept = false
			};
			File.WriteAllText(Constants.AcceptFilePath, JsonConvert.SerializeObject(accept));
		}

	}
	[AutomaticTask("Anonymizer Task",
				   "Protect Data",
				   "Protect data during the project, with or without encryption",
				   GeneratedFileType = AutomaticTaskFileType.BilingualTarget)]
	[AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
	[RequiresSettings(typeof(AnonymizerSettings), typeof(AnonymizerSettingsPage))]
	public class AnonymizerTask : AbstractFileContentProcessingAutomaticTask
	{
		private AnonymizerSettings _settings;
		protected override void OnInitializeTask()
		{
			_settings = GetSetting<AnonymizerSettings>();
		}


		protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
		{
			var projectController = SdlTradosStudio.Application.GetController<ProjectsController>();
			var selectedPatternsFromGrid = _settings.RegexPatterns.Where(e => e.ShouldEnable).ToList();
			if (projectController.CurrentProject != null)
			{
				ProjectBackup.CreateProjectBackup(projectController.CurrentProject.FilePath);
			}
			multiFileConverter.AddBilingualProcessor(new BilingualContentHandlerAdapter(new AnonymizerPreProcessor(selectedPatternsFromGrid,_settings.EncryptionKey)));
		
		}

		public override bool OnFileComplete(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
		{
			return true;
		}
	}

	//Decrypt  task
	[AutomaticTask("Decrypt Task",
		"Unprotect Data",
		"Unprotect data in preparation for saving the target files",
		GeneratedFileType = AutomaticTaskFileType.BilingualTarget)]
	[AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
	[RequiresSettings(typeof(DecryptSettings), typeof(DecryptSettingsPage))]
	public class DecryptTask : AbstractFileContentProcessingAutomaticTask
	{
		private DecryptSettings _settings;
		protected override void OnInitializeTask()
		{
			_settings = GetSetting<DecryptSettings>();
		}

		protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
		{
			var encryptSettings = GetSetting<AnonymizerSettings>();
			//process only if the decryption key matches encryption key
			if (encryptSettings.EncryptionKey.Equals(_settings.EncryptionKey))
			{
				multiFileConverter.AddBilingualProcessor(new BilingualContentHandlerAdapter(new DecryptDataProcessor(_settings)));
			}
			
		}

		public override bool OnFileComplete(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
		{
			return true;
		}
	}
	//[Action("Anonymizer Action",
	//	Name = "Decrypt data",
	//	Description = "Deanonymize data which was previously anonymize by the batch task",
	//	Icon = "unlock"
	//)]
	//[ActionLayout(typeof(TranslationStudioDefaultContextMenus.ProjectsContextMenuLocation), 2, DisplayType.Default, "",
	//	true)]
	//public class AnonymizerDeanonymizeAction : AbstractViewControllerAction<ProjectsController>
	//{
		
	//	protected override void Execute()
	//	{
	//		var selectedProjects = Controller.SelectedProjects.ToList();
	//		var fileTypeManager = DefaultFileTypeManager.CreateInstance(true);
	//		foreach (var project in selectedProjects)
	//		{
	//			var targetFiles = project.GetTargetLanguageFiles();
	//			foreach (var targetFile in targetFiles.ToList())
	//			{
	//				var converter =
	//					fileTypeManager.GetConverterToDefaultBilingual(targetFile.LocalFilePath, targetFile.LocalFilePath, null);
	//				converter.AddBilingualProcessor(new BilingualContentHandlerAdapter(new DecryptDataProcessor()));
	//				converter.Parse();
	//			}
				
	//		}
	//	}
	//}
	[Action("Help Anonymizer Action",
		Name = "Help",
		Description = "Help",
		Icon = "question"
	)]
	[ActionLayout(typeof(TranslationStudioDefaultContextMenus.ProjectsContextMenuLocation), 2, DisplayType.Default, "",
		true)]
	public class AnonymizerHelpAction : AbstractAction
	{
		protected override void Execute()
		{
			System.Diagnostics.Process.Start("https://community.sdl.com/product-groups/translationproductivity/w/customer-experience/3199.gdpr");
		}
	}
}
