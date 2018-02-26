﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.DefaultLocations;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using Sdl.TranslationStudioAutomation.IntegrationApi.Presentation.DefaultLocations;

namespace Sdl.Community.AhkPlugin
{
	[RibbonGroup("Sdl.Community.AhkRibbonGroup", Name = "Ahk", ContextByType = typeof(ProjectsController))]
	[RibbonGroupLayout(LocationByType = typeof(TranslationStudioDefaultRibbonTabs.HomeRibbonTabLocation))]
	[RibbonGroupLayout(LocationByType = typeof(StudioDefaultRibbonTabs.AddinsRibbonTabLocation))]
	public class AhkRibbonGroup: AbstractRibbonGroup
	{
	}

	[Action("Sdl.Community.AhkAction", Name = "AutoHotKey scripts", Icon = "icon", Description = "Manage HotKey Scripts")]
	[ActionLayout(typeof(AhkRibbonGroup), 250, DisplayType.Large)]
	public class AhkAction : AbstractAction
	{
		protected override void Execute()
		{
			var window = new Ui.ScriptsWindow();
			window.ShowDialog();
		}
	}
	[Action("Sdl.Community.HelpAhkAction", Name = "AutoHotKey scripts help", Icon = "question", Description = "An wiki page will be opened in browser uith user documentation")]
	[ActionLayout(typeof(AhkRibbonGroup), 250, DisplayType.Large)]
	public class AhkHelpAction : AbstractAction
	{
		protected override void Execute()
		{
			System.Diagnostics.Process.Start("https://community.sdl.com/product-groups/translationproductivity/w/customer-experience/3171.ahk-plugin");

		}
	}
}