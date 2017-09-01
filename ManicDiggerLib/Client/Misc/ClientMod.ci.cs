public abstract class ClientMod
{
	public virtual void Start(ClientModManager modmanager) { }

	public virtual void OnReadOnlyMainThread(Game game, float dt) { }
	public virtual void OnReadOnlyBackgroundThread(Game game, float dt) { }
	public virtual void OnReadWriteMainThread(Game game, float dt) { }

	public virtual bool OnClientCommand(Game game, ClientCommandArgs args) { return false; }
	public virtual void OnNewFrame(Game game, NewFrameEventArgs args) { }
	public virtual void OnNewFrameFixed(Game game, NewFrameEventArgs args) { }
	public virtual void OnNewFrameDraw2d(Game game, float deltaTime) { }
	public virtual void OnBeforeNewFrameDraw3d(Game game, float deltaTime) { }
	public virtual void OnNewFrameDraw3d(Game game, float deltaTime) { }
	public virtual void OnNewFrameReadOnlyMainThread(Game game, float deltaTime) { }
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
	public virtual void Dispose(Game game) { }
}
