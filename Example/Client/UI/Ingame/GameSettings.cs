using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using System;
using System.Reflection;
using System.Linq;
using Framework;
using Framework.Game.Client;

namespace Shooter.Client.UI.Ingame
{
	public partial class GameSettings : CanvasLayer, IChildComponent
	{
		public IBaseComponent BaseComponent { get; set; }

		public delegate void DisconnectEvent();
		public event DisconnectEvent OnDisconnect;

		[Export]
		NodePath sensPathX = null;

		[Export]
		NodePath sensPathY = null;

		SpinBox sensX = null;
		SpinBox sensY = null;
		SpinBox fov = null;


		[Export]
		NodePath shadowQualityPath = null;

		[Export]
		NodePath fovPath = null;

		[Export]
		NodePath soundVolumePath = null;

		[Export]
		NodePath keyContainerPath = null;


		[Export]
		NodePath debandingPath = null;

		[Export]
		NodePath occlusionPath = null;

		[Export]
		NodePath resChangerPath = null;

		[Export]
		NodePath windowModeChangerPath = null;

		[Export]
		NodePath keyChangeDialogPath = null;

		[Export]
		NodePath closeButtonPath = null;

		[Export]
		NodePath msaaChangerPath = null;

		[Export]
		NodePath debugChangerPath = null;

		[Export]
		NodePath viewportScaleChangerPath = null;

		[Export]
		NodePath aaChangerPath = null;

		[Export]
		NodePath ssaoPath = null;

		[Export]
		NodePath ssilPath = null;

		[Export]
		NodePath glowPath = null;

		[Export]
		NodePath sdfgiPlath = null;

		[Export]
		NodePath vsyncPath = null;

		TabContainer container = null;

		Button closeButton = null;
		VBoxContainer keyListContainer = null;
		KeyConfirmationDialog keyChangeDialog = null;

		OptionButton aaChanger = null;
		OptionButton debugChanger = null;
		OptionButton resChanger = null;
		OptionButton windowModeChanger = null;
		OptionButton msaaChanger = null;
		CheckButton debanding = null;
		CheckButton Vsync = null;

		CheckButton ssao = null;
		CheckButton ssil = null;
		CheckButton sdfgi = null;
		CheckButton glow = null;

		CheckButton occlusionCulling = null;
		OptionButton shadowQuality = null;


		HSlider volumeSlider;
		HSlider viewportScaleChanger;


		public override void _Ready()
		{

			this.sensX = GetNode(sensPathX) as SpinBox;
			this.sensY = GetNode(sensPathY) as SpinBox;
			this.fov = GetNode(fovPath) as SpinBox;

			this.viewportScaleChanger = GetNode<HSlider>(viewportScaleChangerPath);

			this.occlusionCulling = GetNode(occlusionPath) as CheckButton;
			this.ssao = GetNode(ssaoPath) as CheckButton;
			this.ssil = GetNode(ssilPath) as CheckButton;
			this.sdfgi = GetNode(sdfgiPlath) as CheckButton;
			this.glow = GetNode(glowPath) as CheckButton;
			this.debanding = GetNode(debandingPath) as CheckButton;
			this.Vsync = GetNode(vsyncPath) as CheckButton;

			this.shadowQuality = GetNode(shadowQualityPath) as OptionButton;
			this.closeButton = GetNode(closeButtonPath) as Button;
			this.resChanger = GetNode(resChangerPath) as OptionButton;
			this.windowModeChanger = GetNode(windowModeChangerPath) as OptionButton;
			this.msaaChanger = GetNode(msaaChangerPath) as OptionButton;
			this.debugChanger = GetNode(debugChangerPath) as OptionButton;
			this.aaChanger = GetNode(aaChangerPath) as OptionButton;
			this.volumeSlider = GetNode(soundVolumePath) as HSlider;
			//  this.GetNode<ColorRect>("Blur").Visible = false;

			//    this.sensX.Value = ConfigValues.sensitivityX;
			//    this.sensY.Value = ConfigValues.sensitivityY;
			//     this.fov.Value = ConfigValues.fov;

			this.fov.ValueChanged += (float val) =>
			{
				//   ConfigValues.setFov(val);
			};

			this.sensX.ValueChanged += (float val) =>
			{
				//   ConfigValues.setSensitivityX(val);
			};

			this.sensY.ValueChanged += (float val) =>
			{
				// ConfigValues.setSensitivityY(val);
			};

			//  this.shadowQuality.Selected = ConfigValues.shadowQuality;
			//     this.windowModeChanger.Selected = ConfigValues.mode;
			//   this.msaaChanger.Selected = (int)ConfigValues.msaa;
			//      this.aaChanger.Selected = (int)ConfigValues.aa;


			this.InitResolutions();

			this.InitEnums<ClientSettings.WindowModes>(this.windowModeChanger, "cl_window_mode");
			this.InitEnums<Godot.Viewport.MSAA>(this.msaaChanger, "cl_draw_msaa");
			this.InitEnums<Godot.Viewport.ScreenSpaceAA>(this.aaChanger, "cl_draw_aa");
			this.InitEnums<Godot.Viewport.DebugDrawEnum>(this.debugChanger, "cl_draw_debug");
			this.InitEnums<RenderingServer.ShadowQuality>(this.shadowQuality, "cl_draw_shadow");
			this.InitBoolean(this.glow, "cl_draw_glow");
			this.InitBoolean(this.sdfgi, "cl_draw_sdfgi");
			this.InitBoolean(this.ssao, "cl_draw_ssao");
			this.InitBoolean(this.ssil, "cl_draw_ssil");
			this.InitBoolean(this.occlusionCulling, "cl_draw_occulision");
			this.InitBoolean(this.debanding, "cl_draw_debanding");
			this.InitBoolean(this.Vsync, "cl_draw_vsync");

			this.keyListContainer = GetNode(keyContainerPath) as VBoxContainer;
			this.keyChangeDialog = GetNode(keyChangeDialogPath) as KeyConfirmationDialog;
			this.getCurentList();

			this.closeButton.Pressed += () =>
			{
				this.BaseComponent.Components.AddComponent<MenuComponent>("res://Client/UI/Ingame/MenuComponent.tscn");
				this.BaseComponent.Components.DeleteComponent<GameSettings>();

				ClientSettings.Variables.StoreConfig("client.cfg");
			};

			this.keyChangeDialog.Confirmed += () =>
			{
				var node = this.keyListContainer.GetNode(this.keyChangeDialog.keyName) as GameKeyRecord;

				node.currentKey = this.keyChangeDialog.selectedKey;
				ClientSettings.Variables.Set(this.keyChangeDialog.keyName, node.currentKey.ToString());
			};


			this.SetProcess(false);

			var bus = AudioServer.GetBusIndex("Master");

			this.volumeSlider.MinValue = 0f;
			this.volumeSlider.MaxValue = 1.0f;
			this.volumeSlider.Step = 0.05f;

			//    this.volumeSlider.Value = FPS.Game.Config.ConfigValues.masterVolume;
			this.volumeSlider.ValueChanged += (float value) =>
			{
				//   FPS.Game.Config.ConfigValues.setMasterVolume(value);
			};

			this.viewportScaleChanger.MinValue = 0.1f;
			this.viewportScaleChanger.MaxValue = 2.0f;
			this.viewportScaleChanger.Step = 0.05f;

			/*  this.viewportScaleChanger.Value = FPS.Game.Config.ConfigValues.viewportScale;
			  this.viewportScaleChanger.ValueChanged += (float value) =>
			  {
				  FPS.Game.Config.ConfigValues.ApplyScale(currentGameLevel, value);
			  };
			  */
		}

		private void InitResolutions()
		{
			var possibleResolitions = ClientSettings.Resolutions;
			int i = 0;
			int selected = 0;
			foreach (var item in possibleResolitions)
			{
				if (ClientSettings.Variables.GetValue("cl_resolution") == item)
				{
					selected = i;
				}

				this.resChanger.AddItem(item, i++);
			}

			this.resChanger.Selected = selected;
			this.resChanger.ItemSelected += index =>
			{
				ClientSettings.Variables.Set("cl_resolution", ClientSettings.Resolutions[index]);
			};
		}

		private void InitBoolean(CheckButton button, string storeKey)
		{
			var isChecked = ClientSettings.Variables.Get<bool>(storeKey);

			button.ButtonPressed = isChecked;
			button.Pressed += () =>
			{
				ClientSettings.Variables.Set(storeKey, button.ButtonPressed.ToString());
			};
		}

		private void InitEnums<TEnum>(OptionButton button, string storeKey) where TEnum : struct
		{
			var currentWindowMode = ClientSettings.Variables.GetValue(storeKey);

			int i = 0;
			int selectedId = 0;
			foreach (var item in Enum.GetValues(typeof(TEnum)))
			{
				button.AddItem(item.ToString(), (int)item);
				button.SetItemMetadata((int)item, item.ToString());

				if (currentWindowMode == item.ToString())
				{
					selectedId = i;
				}

				i++;
			}

			button.Selected = selectedId;
			button.ItemSelected += index =>
			{
				var meta = button.GetItemMetadata(index);
				ClientSettings.Variables.Set(storeKey, meta.ToString());
			};
		}




		private void getCurentList()
		{
			foreach (var n in Framework.Game.Client.ClientSettings.Variables.Vars.AllVariables.Where(df => df.Key.Contains("key_")))
			{
				var scene = (PackedScene)ResourceLoader.Load("res://Client/UI/Ingame/GameKeyRecord.tscn");
				var record = (GameKeyRecord)scene.Instantiate();

				record.currentKey = n.Value;
				record.Name = n.Key;
				record.OnKeyChangeStart += this.onKeyChangeStart;
				this.keyListContainer.AddChild(record);
			}
		}

		private void onKeyChangeStart(string keyName)
		{
			this.keyChangeDialog.openChanger(keyName);
		}
	}
}
