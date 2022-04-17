using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using System;
using System.Reflection;
using System.Linq;
using Framework;

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
		NodePath glowPath = null;

		[Export]
		NodePath sdfgiPlath = null;

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

		CheckButton ssao = null;
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
			this.sdfgi = GetNode(sdfgiPlath) as CheckButton;
			this.glow = GetNode(glowPath) as CheckButton;
			this.debanding = GetNode(debandingPath) as CheckButton;
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

			this.resChanger.ItemSelected += onResChanged;
			this.windowModeChanger.ItemSelected += onFullScreenChanged;
			this.msaaChanger.ItemSelected += onMsaaChanged;
			this.debanding.Pressed += onDebanding;
			this.debanding.Pressed += onOcclusionCulling;
			this.debugChanger.ItemSelected += onDebugChanger;
			this.aaChanger.ItemSelected += onAaChanger;
			this.shadowQuality.ItemSelected += onShadowQualityChanger;

			//  this.shadowQuality.Selected = ConfigValues.shadowQuality;
			//     this.windowModeChanger.Selected = ConfigValues.mode;
			//   this.msaaChanger.Selected = (int)ConfigValues.msaa;
			//      this.aaChanger.Selected = (int)ConfigValues.aa;
			/*
						foreach (var item in ConfigValues.Resolutions)
						{
							var index = ConfigValues.Resolutions.IndexOf(item);
							this.resChanger.AddItem(item, index);
						}
						*/
			/*
						foreach (var item in ConfigValues.ShadowQualities)
						{
							var index = ConfigValues.ShadowQualities.IndexOf(item);
							this.shadowQuality.AddItem(item, index);
						}
			*/
			foreach (var item in Enum.GetValues(typeof(Godot.Window.ModeEnum)))
			{
				this.windowModeChanger.AddItem(item.ToString(), (int)item);
				this.windowModeChanger.SetItemMetadata((int)item, item);
			}

			foreach (var item in Enum.GetValues(typeof(Godot.Viewport.MSAA)))
			{
				this.msaaChanger.AddItem(item.ToString(), (int)item);
			}

			foreach (var item in Enum.GetValues(typeof(Godot.Viewport.ScreenSpaceAA)))
			{
				this.aaChanger.AddItem(item.ToString(), (int)item);
			}

			foreach (var item in Enum.GetValues(typeof(Godot.Viewport.DebugDrawEnum)))
			{
				this.debugChanger.AddItem(item.ToString(), (int)item);
			}

			//    this.debugChanger.Selected = (int)ConfigValues.debugDrawMode;
			//   this.occlusionCulling.ButtonPressed = ConfigValues.useOcclusionCulling;
			//   this.debanding.ButtonPressed = ConfigValues.useDebanding;

			//  this.ssao.ButtonPressed = ConfigValues.useSSAO;
			//  this.sdfgi.ButtonPressed = ConfigValues.useSDFGI;
			//  this.glow.ButtonPressed = ConfigValues.useGlow;

			this.glow.Pressed += onGlow;
			this.sdfgi.Pressed += onSDFGI;
			this.ssao.Pressed += onSSAO;

			this.keyListContainer = GetNode(keyContainerPath) as VBoxContainer;
			this.keyChangeDialog = GetNode(keyChangeDialogPath) as KeyConfirmationDialog;
			this.getCurentList();


			this.closeButton.Pressed += () =>
			{
				this.BaseComponent.Components.AddComponent<MenuComponent>("res://Client/UI/Ingame/MenuComponent.tscn");
				this.BaseComponent.Components.DeleteComponent<GameSettings>();

				Framework.Game.Client.ClientSettings.Variables.StoreConfig("client.cfg");
			};

			this.keyChangeDialog.Confirmed += () =>
			{
				var node = this.keyListContainer.GetNode(this.keyChangeDialog.keyName) as GameKeyRecord;

				node.currentKey = this.keyChangeDialog.selectedKey;
				Framework.Game.Client.ClientSettings.Variables.Set(this.keyChangeDialog.keyName, node.currentKey.ToString());
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

		public void onSSAO()
		{
			// if (currentGameLevel != null)
			//     ConfigValues.ApplySSAO(currentGameLevel, this.ssao.ButtonPressed);
		}

		public void onSDFGI()
		{
			// if (currentGameLevel != null)
			//     ConfigValues.ApplySDFGI(currentGameLevel, this.sdfgi.ButtonPressed);
		}

		public void onGlow()
		{
			//  if (currentGameLevel != null)
			//    ConfigValues.ApplyGlow(currentGameLevel, this.glow.ButtonPressed);
		}

		public void onOcclusionCulling()
		{
			// ConfigValues.ApplyOcclusionMode(this, this.occlusionCulling.ButtonPressed);
		}

		public void onDebanding()
		{
			//  ConfigValues.ApplyDebanding(this, this.debanding.ButtonPressed);
		}

		public void onShadowQualityChanger(int index)
		{
			//  ConfigValues.ApplyShadowQuality(this, index);
		}

		public void onDebugChanger(int index)
		{
			//  ConfigValues.ApplyDebugMode(this, index);
		}

		public void onAaChanger(int index)
		{
			// ConfigValues.ApplyAA(this, index);
		}

		public void onResChanged(int index)
		{
			//   ConfigValues.ApplyResolution(index);
		}

		public void onFullScreenChanged(int index)
		{
			//   ConfigValues.ApplyWindowMode(this, index);
		}

		public void onMsaaChanged(int index)
		{
			// ConfigValues.ApplyMSAA(this, index);
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
