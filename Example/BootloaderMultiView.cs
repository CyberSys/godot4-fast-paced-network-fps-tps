using System;
using System.Linq;
using Godot;

public partial class BootloaderMultiView : Node
{
	[Export]
	public Godot.Collections.Array<NodePath> viewSlot1 = new Godot.Collections.Array<NodePath>();

	public int viewSlot = 0;

	public override void _EnterTree()
	{
		base._EnterTree();
		this.setGuiInputs();

		this.ProcessMode = ProcessModeEnum.Always;

		GD.Load<CSharpScript>("res://Shared/MyGameLevel.cs");
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event.IsActionReleased("switch_view"))
		{
			if (viewSlot == viewSlot1.Count - 1)
			{
				viewSlot = 0;
			}
			else
			{
				viewSlot++;
			}

			Console.WriteLine("Viewport set to slot " + viewSlot);
			this.setGuiInputs();
		}


		@event.Dispose();
	}

	public void setGuiInputs()
	{
		int i = 0;
		foreach (var item in this.viewSlot1)
		{
			var node = this.GetNodeOrNull<Viewport>(item);
			if (node != null)
			{
				node.GuiDisableInput = (i != viewSlot);
			}

			i++;
		}
	}
}
