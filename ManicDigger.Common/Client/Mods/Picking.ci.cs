public class ModPicking : ClientMod
{
	public ModPicking()
	{
		unproject = new Unproject();
		tempViewport = new float[4];
		tempRay = new float[4];
		tempRayStartPoint = new float[4];
		fillarea = new DictionaryVector3Float();
	}

	public override void OnBeforeNewFrameDraw3d(Game game, float deltaTime)
	{
		if (game.guistate == GuiState.Normal)
		{
			UpdatePicking(game);
		}
	}

	public override void OnMouseUp(Game game, MouseEventArgs args)
	{
		if (game.guistate == GuiState.Normal)
		{
			UpdatePicking(game);
		}
	}

	public override void OnMouseDown(Game game, MouseEventArgs args)
	{
		if (game.guistate == GuiState.Normal)
		{
			UpdatePicking(game);
			UpdateEntityHit(game);
		}
	}

	internal void UpdatePicking(Game game)
	{
		if (game.FollowId() != null)
		{
			game.SelectedBlockPosition.x = 0 - 1;
			game.SelectedBlockPosition.y = 0 - 1;
			game.SelectedBlockPosition.z = 0 - 1;
			return;
		}
	    BlockInteractions(game);
	}

	internal void BlockInteractions(Game game)
	{
		float one = 1;
		bool left = game.mouseLeft;
		bool middle = game.mouseMiddle;
		bool right = game.mouseRight;


		if (!game.leftpressedpicking)
		{
			if (game.mouseleftclick)
			{
				game.leftpressedpicking = true;
			}
			else
			{
				left = false;
			}
		}
		else
		{
			if (game.mouseleftdeclick)
			{
				game.leftpressedpicking = false;
				left = false;
			}
		}
		if (!left)
		{
			game.currentAttackedBlock = null;
		}

		Packet_Item item = game.d_Inventory.RightHand[game.ActiveHudIndex];
	
		
		//grenade cooking - TODO: fix instant explosion when closing ESC menu
		
		
		IntRef pick2count = new IntRef();
		Line3D pick = new Line3D();
		GetPickingLine(game, pick, false);
		BlockPosSide[] pick2 = game.Pick(game.s, pick, pick2count);

		if (left)
		{
			game.handSetAttackDestroy = true;
		}
		else if (right)
		{
			game.handSetAttackBuild = true;
		}

        //Controle 3person camera
		if (game.overheadcamera && pick2count.value > 0 && left)
		{
			//if not picked any object, and mouse button is pressed, then walk to destination.
			if (game.Follow == null)
			{
				//Only walk to destination when not following someone
				game.playerdestination = Vector3Ref.Create(pick2[0].blockPos[0], pick2[0].blockPos[1] + 1, pick2[0].blockPos[2]);
			}
		}


		bool pickdistanceok = (pick2count.value > 0); //&& (!ispistol);
		if (pickdistanceok)
		{
			if (game.Dist(pick2[0].blockPos[0] + one / 2, pick2[0].blockPos[1] + one / 2, pick2[0].blockPos[2] + one / 2,
				pick.Start[0], pick.Start[1], pick.Start[2]) > CurrentPickDistance(game))
			{
				pickdistanceok = false;
			}
		}
		bool playertileempty = game.IsTileEmptyForPhysics(
				  game.platform.FloatToInt(game.player.position.x),
				  game.platform.FloatToInt(game.player.position.z),
				  game.platform.FloatToInt(game.player.position.y + (one / 2)));
		bool playertileemptyclose = game.IsTileEmptyForPhysicsClose(
				  game.platform.FloatToInt(game.player.position.x),
				  game.platform.FloatToInt(game.player.position.z),
				  game.platform.FloatToInt(game.player.position.y + (one / 2)));
		BlockPosSide pick0 = new BlockPosSide();
		if (pick2count.value > 0 &&
			((pickdistanceok && (playertileempty || (playertileemptyclose)))
			|| game.overheadcamera)
			)
		{
			game.SelectedBlockPosition.x = game.platform.FloatToInt(pick2[0].Current()[0]);
			game.SelectedBlockPosition.y = game.platform.FloatToInt(pick2[0].Current()[1]);
			game.SelectedBlockPosition.z = game.platform.FloatToInt(pick2[0].Current()[2]);
			pick0 = pick2[0];
		}
		else
		{
			game.SelectedBlockPosition.x = -1;
			game.SelectedBlockPosition.y = -1;
			game.SelectedBlockPosition.z = -1;
			pick0.blockPos = new float[3];
			pick0.blockPos[0] = -1;
			pick0.blockPos[1] = -1;
			pick0.blockPos[2] = -1;
		}
		PickEntity(game, pick, pick2, pick2count);

		if (game.cameratype == CameraType.Fpp || game.cameratype == CameraType.Tpp)
		{
			int ntileX = game.platform.FloatToInt(pick0.Current()[0]);
			int ntileY = game.platform.FloatToInt(pick0.Current()[1]);
			int ntileZ = game.platform.FloatToInt(pick0.Current()[2]);
			if (game.IsUsableBlock(game.map.GetBlock(ntileX, ntileZ, ntileY)))
			{
				game.currentAttackedBlock = Vector3IntRef.Create(ntileX, ntileZ, ntileY);
			}
		}
		if (game.GetFreeMouse())
		{
			if (pick2count.value > 0)
			{
				OnPick_(pick0);
			}
			return;
		}

		if ((one * (game.platform.TimeMillisecondsFromStart() - lastbuildMilliseconds) / 1000) >= BuildDelay(game)
			)
		{
			if (left && game.d_Inventory.RightHand[game.ActiveHudIndex] == null)
			{
				game.SendPacketClient(ClientPackets.MonsterHit(game.platform.FloatToInt(2 + game.rnd.NextFloat() * 4)));
			}
			if (left && !fastclicking)
			{
				//TODO: animation
				fastclicking = false;
			}
			if ((left || right || middle) )
			{
				lastbuildMilliseconds = game.platform.TimeMillisecondsFromStart();
			}

			if (pick2count.value > 0)
			{
				if (middle)
				{
					int newtileX = game.platform.FloatToInt(pick0.Current()[0]);
					int newtileY = game.platform.FloatToInt(pick0.Current()[1]);
					int newtileZ = game.platform.FloatToInt(pick0.Current()[2]);
					if (game.map.IsValidPos(newtileX, newtileZ, newtileY))
					{
						int clonesource = game.map.GetBlock(newtileX, newtileZ, newtileY);
						int clonesource2 = game.d_Data.WhenPlayerPlacesGetsConvertedTo()[clonesource];
						bool gotoDone = false;
						//find this block in another right hand.
						for (int i = 0; i < 10; i++)
						{
							if (game.d_Inventory.RightHand[i] != null
								&& game.d_Inventory.RightHand[i].ItemClass == Packet_ItemClassEnum.Block
								&& game.d_Inventory.RightHand[i].BlockId == clonesource2)
							{
								game.ActiveHudIndex = i;
								gotoDone = true;
							}
						}
						if (!gotoDone)
						{
							IntRef freehand = game.d_InventoryUtil.FreeHand(game.ActiveHudIndex);
							//find this block in inventory.
							for (int i = 0; i < game.d_Inventory.ItemsCount; i++)
							{
								Packet_PositionItem k = game.d_Inventory.Items[i];
								if (k == null)
								{
									continue;
								}
								if (k.Value_.ItemClass == Packet_ItemClassEnum.Block
									&& k.Value_.BlockId == clonesource2)
								{
									//free hand
									if (freehand != null)
									{
										game.WearItem(
											game.InventoryPositionMainArea(k.X, k.Y),
											game.InventoryPositionMaterialSelector(freehand.value));
										break;
									}
									//try to replace current slot
									if (game.d_Inventory.RightHand[game.ActiveHudIndex] != null
										&& game.d_Inventory.RightHand[game.ActiveHudIndex].ItemClass == Packet_ItemClassEnum.Block)
									{
										game.MoveToInventory(
											game.InventoryPositionMaterialSelector(game.ActiveHudIndex));
										game.WearItem(
											game.InventoryPositionMainArea(k.X, k.Y),
											game.InventoryPositionMaterialSelector(game.ActiveHudIndex));
									}
								}
							}
						}
						string[] sound = game.d_Data.CloneSound()[clonesource];
						if (sound != null) // && sound.Length > 0)
						{
							game.AudioPlay(game.platform.StringFormat("{0}.ogg", sound[0])); //TODO: sound cycle
						}
					}
				}
				if (left || right)
				{
					BlockPosSide tile = pick0;
					int newtileX;
					int newtileY;
					int newtileZ;
					if (right)
					{
						newtileX = game.platform.FloatToInt(tile.Translated()[0]);
						newtileY = game.platform.FloatToInt(tile.Translated()[1]);
						newtileZ = game.platform.FloatToInt(tile.Translated()[2]);
					}
					else
					{
						newtileX = game.platform.FloatToInt(tile.Current()[0]);
						newtileY = game.platform.FloatToInt(tile.Current()[1]);
						newtileZ = game.platform.FloatToInt(tile.Current()[2]);
					}
					if (game.map.IsValidPos(newtileX, newtileZ, newtileY))
					{
						//Console.WriteLine(". newtile:" + newtile + " type: " + d_Map.GetBlock(newtileX, newtileZ, newtileY));
						if (!(pick0.blockPos[0] == -1
							 && pick0.blockPos[1] == -1
							&& pick0.blockPos[2] == -1))
						{
							int blocktype;
							if (left) { blocktype = game.map.GetBlock(newtileX, newtileZ, newtileY); }
							else { blocktype = ((game.BlockInHand() == null) ? 1 : game.BlockInHand().value); }
							if (left && blocktype == game.d_Data.BlockIdAdminium())
							{
								PickingEnd(left, right, middle, false);
								return;
							}
							string[] sound = left ? game.d_Data.BreakSound()[blocktype] : game.d_Data.BuildSound()[blocktype];
							if (sound != null) // && sound.Length > 0)
							{
								game.AudioPlay(game.platform.StringFormat("{0}.ogg", sound[0])); //TODO: sound cycle
							}
						}
						//normal attack
						if (!right)
						{
							//attack
							int posx = newtileX;
							int posy = newtileZ;
							int posz = newtileY;

							game.currentAttackedBlock = Vector3IntRef.Create(posx, posy, posz);
							if (!game.blockHealth.ContainsKey(posx, posy, posz))
							{
								game.blockHealth.Set(posx, posy, posz, game.GetCurrentBlockHealth(posx, posy, posz));
							}
                            float toolStrenght = game.GetToolStrenght(item.BlockId, game.map.GetBlock(posx, posy, posz));
                            game.blockHealth.Set(posx, posy, posz, game.blockHealth.Get(posx, posy, posz) - toolStrenght);
                             


                            float health = game.GetCurrentBlockHealth(posx, posy, posz);
							if (health <= 0)
							{
								if (game.currentAttackedBlock != null)
								{
									game.blockHealth.Remove(posx, posy, posz);
								}
								game.currentAttackedBlock = null;
								OnPick(game, game.platform.FloatToInt(newtileX), game.platform.FloatToInt(newtileZ), game.platform.FloatToInt(newtileY),
									game.platform.FloatToInt(tile.Current()[0]), game.platform.FloatToInt(tile.Current()[2]), game.platform.FloatToInt(tile.Current()[1]),
									tile.collisionPos,
									right);
							}
							PickingEnd(left, right, middle, false);
							return;
						}
						if (!right)
						{
							game.particleEffectBlockBreak.StartParticleEffect(newtileX, newtileY, newtileZ);//must be before deletion - gets ground type.
						}
						if (!game.map.IsValidPos(newtileX, newtileZ, newtileY))
						{
							game.platform.ThrowException("Error in picking - NextBullet()");
						}
						OnPick(game, game.platform.FloatToInt(newtileX), game.platform.FloatToInt(newtileZ), game.platform.FloatToInt(newtileY),
							game.platform.FloatToInt(tile.Current()[0]), game.platform.FloatToInt(tile.Current()[2]), game.platform.FloatToInt(tile.Current()[1]),
							tile.collisionPos,
							right);
						//network.SendSetBlock(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
						//    right ? BlockSetMode.Create : BlockSetMode.Destroy, (byte)MaterialSlots[activematerial]);
					}
				}
			}
		}
		PickingEnd(left, right, middle, false);
	}

	internal float BuildDelay(Game game)
	{
		float default_ = (1f * 95 / 100) * (1 / game.basemovespeed);
		Packet_Item item = game.d_Inventory.RightHand[game.ActiveHudIndex];
		if (item == null || item.ItemClass != Packet_ItemClassEnum.Block)
		{
			return default_;
		}
		float delay = game.DeserializeFloat(game.blocktypes[item.BlockId].DelayFloat);
		if (delay == 0)
		{
			return default_;
		}
		return delay;
	}

	//value is original block.
	internal DictionaryVector3Float fillarea;
	internal Vector3IntRef fillstart;
	internal Vector3IntRef fillend;

	internal void OnPick(Game game, int blockposX, int blockposY, int blockposZ, int blockposoldX, int blockposoldY, int blockposoldZ, float[] collisionPos, bool right)
	{
		float xfract = collisionPos[0] - game.MathFloor(collisionPos[0]);
		float zfract = collisionPos[2] - game.MathFloor(collisionPos[2]);
		int activeItem= game.HudSlots_(game.ActiveHudIndex);
		int railstart = game.d_Data.BlockIdRailstart();
		if (activeItem == railstart + RailDirectionFlags.TwoHorizontalVertical
			|| activeItem == railstart + RailDirectionFlags.Corners)
		{
			RailDirection dirnew;
			if (activeItem == railstart + RailDirectionFlags.TwoHorizontalVertical)
			{
				dirnew = PickHorizontalVertical(xfract, zfract);
			}
			else
			{
				dirnew = PickCorners(xfract, zfract);
			}
			int dir = game.d_Data.Rail()[game.map.GetBlock(blockposoldX, blockposoldY, blockposoldZ)];
			if (dir != 0)
			{
				blockposX = blockposoldX;
				blockposY = blockposoldY;
				blockposZ = blockposoldZ;
			}
			activeItem = railstart + (dir | DirectionUtils.ToRailDirectionFlags(dirnew));
		}
		int x = game.platform.FloatToInt(blockposX);
		int y = game.platform.FloatToInt(blockposY);
		int z = game.platform.FloatToInt(blockposZ);

		int mode = right ? Packet_BlockSetModeEnum.Create : Packet_BlockSetModeEnum.Destroy;
		{
			if (game.IsAnyPlayerInPos(x, y, z) || activeItem == 151) // Compass
			{
				return;
			}
			Vector3IntRef v = Vector3IntRef.Create(x, y, z);
			Vector3IntRef oldfillstart = fillstart;
			Vector3IntRef oldfillend = fillend;
			if (mode == Packet_BlockSetModeEnum.Create)
			{
				if (game.blocktypes[activeItem].IsTool)
				{
					OnPickUseWithTool(game, blockposX, blockposY, blockposZ);
					return;
				}
                if (!game.blocktypes[activeItem].IsBuildable)
                {
                    return;
                }

                if (activeItem == game.d_Data.BlockIdCuboid())
				{
					ClearFillArea(game);

					if (fillstart != null)
					{
						Vector3IntRef f = fillstart;
						if (!game.IsFillBlock(game.map.GetBlock(f.X, f.Y, f.Z)))
						{
							fillarea.Set(f.X, f.Y, f.Z, game.map.GetBlock(f.X, f.Y, f.Z));
						}
						game.SetBlock(f.X, f.Y, f.Z, game.d_Data.BlockIdFillStart());


						FillFill(game, v, fillstart);
					}
					if (!game.IsFillBlock(game.map.GetBlock(v.X, v.Y, v.Z)))
					{
						fillarea.Set(v.X, v.Y, v.Z, game.map.GetBlock(v.X, v.Y, v.Z));
					}
					game.SetBlock(v.X, v.Y, v.Z, game.d_Data.BlockIdCuboid());
					fillend = v;
					game.RedrawBlock(v.X, v.Y, v.Z);
					return;
				}
				if (activeItem == game.d_Data.BlockIdFillStart())
				{
					ClearFillArea(game);
					if (!game.IsFillBlock(game.map.GetBlock(v.X, v.Y, v.Z)))
					{
						fillarea.Set(v.X, v.Y, v.Z, game.map.GetBlock(v.X, v.Y, v.Z));
					}
					game.SetBlock(v.X, v.Y, v.Z, game.d_Data.BlockIdFillStart());
					fillstart = v;
					fillend = null;
					game.RedrawBlock(v.X, v.Y, v.Z);
					return;
				}
				if (fillarea.ContainsKey(v.X, v.Y, v.Z))// && fillarea[v])
				{
					game.SendFillArea(fillstart.X, fillstart.Y, fillstart.Z, fillend.X, fillend.Y, fillend.Z, activeItem);
					ClearFillArea(game);
					fillstart = null;
					fillend = null;
					return;
				}
			}
			else
			{
				if (game.blocktypes[activeItem].IsTool)
				{
					OnPickUseWithTool(game, blockposX, blockposY, blockposoldZ);
					return;
				}
				//delete fill start
				if (fillstart != null && fillstart.X == v.X && fillstart.Y == v.Y && fillstart.Z == v.Z)
				{
					ClearFillArea(game);
					fillstart = null;
					fillend = null;
					return;
				}
				//delete fill end
				if (fillend != null && fillend.X == v.X && fillend.Y == v.Y && fillend.Z == v.Z)
				{
					ClearFillArea(game);
					fillend = null;
					return;
				}
			}
			game.SendSetBlockAndUpdateSpeculative(activeItem, x, y, z, mode);
		}
	}

	internal void ClearFillArea(Game game)
	{
		for (int i = 0; i < fillarea.itemsCount; i++)
		{
			Vector3Float k = fillarea.items[i];
			if (k == null)
			{
				continue;
			}
			game.SetBlock(k.x, k.y, k.z, game.platform.FloatToInt(k.value));
			game.RedrawBlock(k.x, k.y, k.z);
		}
		fillarea.Clear();
	}

	internal void FillFill(Game game, Vector3IntRef a_, Vector3IntRef b_)
	{
		int startx = MathCi.MinInt(a_.X, b_.X);
		int endx = MathCi.MaxInt(a_.X, b_.X);
		int starty = MathCi.MinInt(a_.Y, b_.Y);
		int endy = MathCi.MaxInt(a_.Y, b_.Y);
		int startz = MathCi.MinInt(a_.Z, b_.Z);
		int endz = MathCi.MaxInt(a_.Z, b_.Z);
		for (int x = startx; x <= endx; x++)
		{
			for (int y = starty; y <= endy; y++)
			{
				for (int z = startz; z <= endz; z++)
				{
					if (fillarea.Count() > game.fillAreaLimit)
					{
						ClearFillArea(game);
						return;
					}
					if (!game.IsFillBlock(game.map.GetBlock(x, y, z)))
					{
						fillarea.Set(x, y, z, game.map.GetBlock(x, y, z));
						game.SetBlock(x, y, z, game.d_Data.BlockIdFillArea());
						game.RedrawBlock(x, y, z);
					}
				}
			}
		}
	}

	internal void OnPickUseWithTool(Game game, int posX, int posY, int posZ)
	{
		game.SendSetBlock(posX, posY, posZ, Packet_BlockSetModeEnum.UseWithTool, game.d_Inventory.RightHand[game.ActiveHudIndex].BlockId, game.ActiveHudIndex);
	}

	internal RailDirection PickHorizontalVertical(float xfract, float yfract)
	{
		float x = xfract;
		float y = yfract;
		if (y >= x && y >= (1 - x))
		{
			return RailDirection.Vertical;
		}
		if (y < x && y < (1 - x))
		{
			return RailDirection.Vertical;
		}
		return RailDirection.Horizontal;
	}

	internal RailDirection PickCorners(float xfract, float zfract)
	{
		float half = 0.5f;
		if (xfract < half && zfract < half)
		{
			return RailDirection.UpLeft;
		}
		if (xfract >= half && zfract < half)
		{
			return RailDirection.UpRight;
		}
		if (xfract < half && zfract >= half)
		{
			return RailDirection.DownLeft;
		}
		return RailDirection.DownRight;
	}

	void PickEntity(Game game, Line3D pick, BlockPosSide[] pick2, IntRef pick2count)
	{
		game.SelectedEntityId = -1;
		game.currentlyAttackedEntity = -1;
		float one = 1;
		for (int i = 0; i < game.entitiesCount; i++)
		{
			if (game.entities[i] == null)
			{
				continue;
			}
			if (i == game.LocalPlayerId)
			{
				continue;
			}
			if (game.entities[i].drawModel == null)
			{
				continue;
			}
			Entity p_ = game.entities[i];
			if (p_.networkPosition == null)
			{
				continue;
			}
			if (!p_.networkPosition.PositionLoaded)
			{
				continue;
			}
			if (!p_.usable)
			{
				continue;
			}
			float feetposX = p_.position.x;
			float feetposY = p_.position.y;
			float feetposZ = p_.position.z;

			float dist = game.Dist(feetposX, feetposY, feetposZ, game.player.position.x, game.player.position.y, game.player.position.z);
			if (dist > 5)
			{
				continue;
			}

			//var p = PlayerPositionSpawn;
			Box3D bodybox = new Box3D();
			float h = p_.drawModel.ModelHeight;
			float r = one * 35 / 100;

			bodybox.AddPoint(feetposX - r, feetposY + 0, feetposZ - r);
			bodybox.AddPoint(feetposX - r, feetposY + 0, feetposZ + r);
			bodybox.AddPoint(feetposX + r, feetposY + 0, feetposZ - r);
			bodybox.AddPoint(feetposX + r, feetposY + 0, feetposZ + r);

			bodybox.AddPoint(feetposX - r, feetposY + h, feetposZ - r);
			bodybox.AddPoint(feetposX - r, feetposY + h, feetposZ + r);
			bodybox.AddPoint(feetposX + r, feetposY + h, feetposZ - r);
			bodybox.AddPoint(feetposX + r, feetposY + h, feetposZ + r);

			float[] p;
			float localeyeposX = game.EyesPosX();
			float localeyeposY = game.EyesPosY();
			float localeyeposZ = game.EyesPosZ();
			p = Intersection.CheckLineBoxExact(pick, bodybox);
			if (p != null)
			{
				//do not allow to shoot through terrain
				if (pick2count.value == 0 || (game.Dist(pick2[0].blockPos[0], pick2[0].blockPos[1], pick2[0].blockPos[2], localeyeposX, localeyeposY, localeyeposZ)
					> game.Dist(p[0], p[1], p[2], localeyeposX, localeyeposY, localeyeposZ)))
				{
					game.SelectedEntityId = i;
					if (game.cameratype == CameraType.Fpp || game.cameratype == CameraType.Tpp)
					{
						game.currentlyAttackedEntity = i;
					}
				}
			}
		}
	}

	void UpdateEntityHit(Game game)
	{
		//Only single hit when mouse clicked
		if (game.currentlyAttackedEntity != -1 && game.mouseLeft)
		{
			for (int i = 0; i < game.clientmodsCount; i++)
			{
				if (game.clientmods[i] == null) { continue; }
				OnUseEntityArgs args = new OnUseEntityArgs();
				args.entityId = game.currentlyAttackedEntity;
				game.clientmods[i].OnHitEntity(game, args);
			}
			game.SendPacketClient(ClientPackets.HitEntity(game.currentlyAttackedEntity));
		}
	}

	internal bool fastclicking;
	internal void PickingEnd(bool left, bool right, bool middle, bool ispistol)
	{
		fastclicking = false;
		if ((!(left || right || middle)) && (!ispistol))
		{
			lastbuildMilliseconds = 0;
			fastclicking = true;
		}
	}

	internal int lastbuildMilliseconds;

	internal void OnPick_(BlockPosSide pick0)
	{
		//playerdestination = pick0.pos;
	}

	Unproject unproject;
	float[] tempViewport;
	float[] tempRay;
	float[] tempRayStartPoint;
	public void GetPickingLine(Game game, Line3D retPick, bool ispistolshoot)
	{
		int mouseX;
		int mouseY;

		if (game.cameratype == CameraType.Fpp || game.cameratype == CameraType.Tpp)
		{
			mouseX = game.Width() / 2;
			mouseY = game.Height() / 2;
		}
		else
		{
			mouseX = game.mouseCurrentX;
			mouseY = game.mouseCurrentY;
		}

		PointFloatRef aim = GetAim(game);
		if (ispistolshoot && (aim.X != 0 || aim.Y != 0))
		{
			mouseX += game.platform.FloatToInt(aim.X);
			mouseY += game.platform.FloatToInt(aim.Y);
		}

		tempViewport[0] = 0;
		tempViewport[1] = 0;
		tempViewport[2] = game.Width();
		tempViewport[3] = game.Height();

		unproject.UnProject(mouseX, game.Height() - mouseY, 1, game.rend.mvMatrix.Peek(), game.rend.pMatrix.Peek(), tempViewport, tempRay);
		unproject.UnProject(mouseX, game.Height() - mouseY, 0, game.rend.mvMatrix.Peek(), game.rend.pMatrix.Peek(), tempViewport, tempRayStartPoint);

		float raydirX = (tempRay[0] - tempRayStartPoint[0]);
		float raydirY = (tempRay[1] - tempRayStartPoint[1]);
		float raydirZ = (tempRay[2] - tempRayStartPoint[2]);
		float raydirLength = game.Length(raydirX, raydirY, raydirZ);
		raydirX /= raydirLength;
		raydirY /= raydirLength;
		raydirZ /= raydirLength;

		retPick.Start = new float[3];
		retPick.Start[0] = tempRayStartPoint[0];// +raydirX; //do not pick behind
		retPick.Start[1] = tempRayStartPoint[1];// +raydirY;
		retPick.Start[2] = tempRayStartPoint[2];// +raydirZ;

		float pickDistance1 = CurrentPickDistance(game) * ((ispistolshoot) ? 100 : 1);
		pickDistance1 += 1;
		retPick.End = new float[3];
		retPick.End[0] = tempRayStartPoint[0] + raydirX * pickDistance1;
		retPick.End[1] = tempRayStartPoint[1] + raydirY * pickDistance1;
		retPick.End[2] = tempRayStartPoint[2] + raydirZ * pickDistance1;
	}

	internal PointFloatRef GetAim(Game game)
	{
		if (game.CurrentAimRadius() <= 1)
		{
			return PointFloatRef.Create(0, 0);
		}
		float half = 0.5f;
		float x;
		float y;
		for (; ; )
		{
			x = (game.rnd.NextFloat() - half) * game.CurrentAimRadius() * 2;
			y = (game.rnd.NextFloat() - half) * game.CurrentAimRadius() * 2;
			float dist1 = game.platform.MathSqrt(x * x + y * y);
			if (dist1 <= game.CurrentAimRadius())
			{
				break;
			}
		}
		return PointFloatRef.Create(x, y);
	}

	float CurrentPickDistance(Game game)
	{
		float pick_distance = game.PICK_DISTANCE;
		IntRef inHand = game.BlockInHand();
		if (inHand != null)
		{
			if (game.blocktypes[inHand.value].PickDistanceWhenUsedFloat > 0)
			{
				// This check ensures that players can select blocks when no value is given
				pick_distance = game.DeserializeFloat(game.blocktypes[inHand.value].PickDistanceWhenUsedFloat);
			}
		}
		if (game.cameratype == CameraType.Tpp)
		{
			pick_distance = game.tppcameradistance + game.PICK_DISTANCE;
		}
		if (game.cameratype == CameraType.Overhead)
		{
			if (game.platform.IsFastSystem())
			{
				pick_distance = 100;
			}
			else
			{
				pick_distance = game.overheadcameradistance * 2;
			}
		}
		return pick_distance;
	}
}
