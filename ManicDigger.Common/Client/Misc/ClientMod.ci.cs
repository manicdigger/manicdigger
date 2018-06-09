public abstract class ClientMod
{
	public virtual void Start(ClientModManager modmanager) { }
	public virtual void Dispose(Game game) { }

	public virtual void OnReadOnlyMainThread(Game game, float dt) { }
	public virtual void OnReadOnlyBackgroundThread(Game game, float dt) { }
	public virtual void OnReadWriteMainThread(Game game, float dt) { }

	/// <summary>
	/// Called after rendering both 3D and 2D parts of a frame has been finished.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="args">Parameters of the rendered frame</param>
	public virtual void OnNewFrame(Game game, NewFrameEventArgs args) { }
	/// <summary>
	/// Called before rendering starts. May be called multiple times in a row.
	/// Called at most 75 times a second. Interval bound to frame rate.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="args">Parameters of the rendered frame</param>
	public virtual void OnNewFrameFixed(Game game, NewFrameEventArgs args) { }
	/// <summary>
	/// Called before drawing 2D content after rendering the 3D scene.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="deltaTime">Milliseconds since the last frame</param>
	public virtual void OnBeforeNewFrameDraw2d(Game game, float deltaTime) { }
	/// <summary>
	/// Called when rendering the 2D part of a frame.
	/// Skipped when UI is hidden!
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="deltaTime">Milliseconds since the last frame</param>
	public virtual void OnNewFrameDraw2d(Game game, float deltaTime) { }
	/// <summary>
	/// Called before rendering 3D content.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="deltaTime">Milliseconds since the last frame</param>
	public virtual void OnBeforeNewFrameDraw3d(Game game, float deltaTime) { }
	/// <summary>
	/// Called when rendering the 3D scene. Skipped when GuiState == MapLoading.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="deltaTime">Milliseconds since the last frame</param>
	public virtual void OnNewFrameDraw3d(Game game, float deltaTime) { }

	public virtual void OnKeyDown(Game game, KeyEventArgs args) { }
	public virtual void OnKeyPress(Game game, KeyPressEventArgs args) { }
	public virtual void OnKeyUp(Game game, KeyEventArgs args) { }
	public virtual void OnMouseUp(Game game, MouseEventArgs args) { }
	public virtual void OnMouseDown(Game game, MouseEventArgs args) { }
	public virtual void OnMouseMove(Game game, MouseEventArgs args) { }
	public virtual void OnMouseWheelChanged(Game game, MouseWheelEventArgs args) { }

	public virtual void OnTouchStart(Game game, TouchEventArgs e) { }
	public virtual void OnTouchMove(Game game, TouchEventArgs e) { }
	public virtual void OnTouchEnd(Game game, TouchEventArgs e) { }
	public virtual void OnUseEntity(Game game, OnUseEntityArgs e) { }
	public virtual void OnHitEntity(Game game, OnUseEntityArgs e) { }

	/// <summary>
	/// Called when the player types a client command in chat.
	/// All messages starting with a "." are considered client commands.
	/// </summary>
	/// <param name="game">Game object</param>
	/// <param name="args">Command parameters</param>
	/// <returns></returns>
	public virtual bool OnClientCommand(Game game, ClientCommandArgs args) { return false; }
}
