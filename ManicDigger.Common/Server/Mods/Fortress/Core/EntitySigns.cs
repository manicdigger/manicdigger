using System;
using System.Drawing;

namespace ManicDigger.Mods
{
	class EntitySigns : IMod
	{
		public void PreStart(ModManager m) { }

		public void Start(ModManager m)
		{
			this.m = m;
			editingSign = new ServerEntityId[m.GetMaxPlayers()];

			m.SetBlockType(75, "EntitySign", new BlockType()
			{
				AllTextures = "VandalFinder",
				DrawType = DrawType.Solid,
				WalkableType = WalkableType.Solid,
				IsUsable = true,
				IsTool = true,
			});
			m.AddToCreativeInventory("EntitySign");

			m.RegisterOnBlockUseWithTool(OnUseWithTool);
			m.RegisterOnEntityUse(OnEntityUse);
			m.RegisterOnEntityUpdate(OnEntityUpdate);
			m.RegisterOnDialogClick2(OnDialogClick);
		}

		ModManager m;
		ServerEntityId[] editingSign;

		private void OnDialogClick(DialogClickArgs args)
		{
			if (args.GetWidgetId() != "UseSign_OK")
			{
				//Return when dialog is not a sign
				return;
			}
			string newText = args.GetTextBoxValue()[1];
			ServerEntityId id = editingSign[args.GetPlayer()];
			if (newText != "")
			{
				editingSign[args.GetPlayer()] = null;
				ServerEntity e = m.EntityGet(id);
				e.sign.text = newText;
				m.EntitySetDirty(id);
			}
			else
			{
				m.EntityDelete(id);
			}
			m.SendDialog(args.GetPlayer(), "UseSign", null);
		}

		private void OnEntityUpdate(ServerEntityId id)
		{
			ServerEntity e = m.EntityGet(id);
			if (e.sign == null)
			{
				return;
			}
			if (e.drawModel == null)
			{
				e.drawModel = new ServerEntityAnimatedModel();
			}
			e.drawModel.model = "signmodel.txt";
			e.drawModel.texture = "signmodel.png";
			e.drawModel.modelHeight = 1.3f;

			if (e.drawText == null)
			{
				e.drawText = new ServerEntityDrawText();
			}
			e.drawText.text = e.sign.text;
			e.drawText.dx = 0.1f;
			e.drawText.dy = 1.1f;
			e.drawText.dz = 0.1f;
			e.usable = true;
			if (e.drawName == null)
			{
				e.drawName = new ServerEntityDrawName();
				e.drawName.name = "Sign";
				e.drawName.onlyWhenSelected = true;
			}
		}

		private void OnEntityUse(int player, ServerEntityId id)
		{
			ServerEntity e = m.EntityGet(id);
			if (e.sign == null)
			{
				return;
			}

			// TODO: Check permissions

			Dialog d = new Dialog();
			d.Width = 400;
			d.Height = 200;
			d.IsModal = true;
			d.Widgets = new Widget[3];
			int widgetCount = 0;
			var font = new DialogFont("Verdana", 11f, DialogFontStyle.Bold);
			d.Widgets[widgetCount++] = Widget.MakeSolid(0, 0, 300, 200, Color.FromArgb(255, 50, 50, 50).ToArgb());
			d.Widgets[widgetCount++] = Widget.MakeTextBox(e.sign.text, font, 50, 50, 200, 50);
			Widget okHandler = Widget.MakeButton("OK", 100, 100, 100, 50);
			okHandler.Clickable = true;
			okHandler.Id = "UseSign_OK";
			d.Widgets[widgetCount++] = okHandler;
			editingSign[player] = id;
			m.SendDialog(player, "UseSign", d);
		}

		private void OnUseWithTool(int player, int x, int y, int z, int tool)
		{
			if (m.GetBlockName(tool) != "EntitySign")
			{
				return;
			}

			// TODO: Check permissions

			ServerEntity e = new ServerEntity();
			e.position = new ServerEntityPositionAndOrientation();
			e.position.x = x + 0.5f;
			e.position.y = z;
			e.position.z = y + 0.5f;
			e.position.heading = GetHeadingLookAt(m.GetPlayerPositionX(player), m.GetPlayerPositionY(player), e.position.x, e.position.z);
			e.sign = new ServerEntitySign();
			e.sign.text = "Hello world!";
			m.EntityCreate(e);
		}

		public static byte GetHeadingLookAt(float posx, float posy, float targetx, float targety)
		{
			float deltaX = targetx - posx;
			float deltaY = targety - posy;
			//Angle to x-axis: cos(beta) = x / |length|
			double headingDeg = (360.0 / (2.0 * Math.PI)) * Math.Acos(deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY)) + 90.0;
			//Add 2 Pi if value is negative
			if (deltaY < 0)
			{
				headingDeg = -headingDeg - 180.0;
			}
			if (headingDeg < 0)
			{
				headingDeg += 360.0;
			}
			if (headingDeg > 360.0)
			{
				headingDeg -= 360.0;
			}
			//Convert to value between 0 and 255 and return
			return (byte)((headingDeg / 360.0) * 256.0);
		}
	}
}
