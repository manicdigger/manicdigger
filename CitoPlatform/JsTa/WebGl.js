function WebGl()
{
}
WebGl.prototype = new Gl();

WebGl.prototype.activeTexture = function(texture) {
context.activeTexture(texture);
}

var onnewframe;
WebGl.prototype.addOnNewFrame = function(handler) {
onnewframe=handler;
}

var onkeyevent;
WebGl.prototype.addOnKeyEvent = function(handler) {
onkeyevent=handler;
document.onkeydown = function(event)
 {
 var args = new KeyEventArgs();
 args.setKeyCode(event.keyCode);
 handler.onKeyDown(args);
 };
document.onkeyup = function(event)
 {
 var args = new KeyEventArgs();
 args.setKeyCode(event.keyCode);
 handler.onKeyUp(args);
 };
document.onkeypress = function(event)
 {
 var args = new KeyPressEventArgs();
 args.setKeyChar(event.charCode);
 handler.onKeyPress(args);
 };
}

var onmouseevent;
WebGl.prototype.addOnMouseEvent = function(handler) {
document.onmousedown = function(event)
{
var args=new MouseEventArgs();
args.setX(event.x);
args.setY(event.y);
args.setButton(event.button);
handler.onMouseDown(args);
};
document.onmouseup = function(event)
{
var args=new MouseEventArgs();
args.setX(event.x);
args.setY(event.y);
args.setButton(event.button);
handler.onMouseUp(args);
};
document.onmousemove = function(event)
{
var args=new MouseEventArgs();
args.setX(event.x);
args.setY(event.y);
handler.onMouseMove(args);
};
document.addEventListener("mousewheel", function(event)
{
var args=new MouseWheelEventArgs();
var delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));
args.setDelta(delta);
args.setDeltaPrecise(delta);
handler.onMouseWheel(args);
});
}

var ontouchevent;
WebGl.prototype.addOnTouchEvent = function(handler) {
document.addEventListener("touchstart", function(event)
{
var touch = event.changedTouches[0];
var args=new TouchEventArgs();
args.setX(touch.pageX);
args.setY(touch.pageY);
args.setId(touch.identifier);
handler.onTouchStart(args);
});
document.addEventListener("touchmove", function(event)
{
var touch = event.changedTouches[0];
var args=new TouchEventArgs();
args.setX(touch.pageX);
args.setY(touch.pageY);
args.setId(touch.identifier);
handler.onTouchEnd(args);
});
document.addEventListener("touchend", function(event)
{
var touch = event.changedTouches[0];
var args=new TouchEventArgs();
args.setX(touch.pageX);
args.setY(touch.pageY);
args.setId(touch.identifier);
handler.onTouchMove(args);
});
}

WebGl.prototype.attachShader = function(program, shader) {
context.attachShader(program, shader);
}

WebGl.prototype.bindAttribLocation = function(program, index, name) {
}

WebGl.prototype.bindBuffer = function(target, buffer) {
context.bindBuffer(target, buffer);
}

WebGl.prototype.bindFramebuffer = function(target, framebuffer) {
}

WebGl.prototype.bindRenderbuffer = function(target, renderbuffer) {
}

WebGl.prototype.bindTexture = function(target, texture) {
context.bindTexture(target, texture);
}

WebGl.prototype.blendColor = function(red, green, blue, alpha) {
}

WebGl.prototype.blendEquation = function(mode) {
}

WebGl.prototype.blendEquationSeparate = function(modeRGB, modeAlpha) {
}

WebGl.prototype.blendFunc = function(sfactor, dfactor) {
}

WebGl.prototype.blendFuncSeparate = function(srcRGB, dstRGB, srcAlpha, dstAlpha) {
}

WebGl.prototype.bufferData1 = function(target, size, usage) {
context.bufferData(target, size, usage);
}

WebGl.prototype.bufferData2 = function(target, data, usage) {
context.bufferData(target, data, usage);
}

WebGl.prototype.bufferData3 = function(target, data, usage) {
context.bufferData(target, data, usage);
}

WebGl.prototype.bufferDataFloat = function(target, data, usage) {
context.bufferData(target, new Float32Array(data), usage);
}

WebGl.prototype.bufferDataUshort = function(target, data, usage) {
context.bufferData(target, new Uint16Array(data), usage);
}

WebGl.prototype.bufferSubData1 = function(target, offset, data) {
}

WebGl.prototype.bufferSubData2 = function(target, offset, data) {
}

WebGl.prototype.checkFramebufferStatus = function(target) {
	return 0;
}

WebGl.prototype.clear = function(mask) {
context.clear(mask);
}

WebGl.prototype.clearColor = function(red, green, blue, alpha) {
context.clearColor(red, green, blue, alpha);
}

WebGl.prototype.clearDepth = function(depth) {
}

WebGl.prototype.clearStencil = function(s) {
}

WebGl.prototype.colorMask = function(red, green, blue, alpha) {
}

WebGl.prototype.compileShader = function(shader) {
context.compileShader(shader);
}

WebGl.prototype.compressedTexImage2D = function(target, level, internalformat, width, height, border, data) {
}

WebGl.prototype.compressedTexSubImage2D = function(target, level, xoffset, yoffset, width, height, format, data) {
}

WebGl.prototype.copyTexImage2D = function(target, level, internalformat, x, y, width, height, border) {
}

WebGl.prototype.copyTexSubImage2D = function(target, level, xoffset, yoffset, x, y, width, height) {
}

WebGl.prototype.createBuffer = function() {
	return context.createBuffer();
}

WebGl.prototype.createFramebuffer = function() {
	return null;
}

function GlImage()
{
this.image = new Image();
}

GlImage.prototype.setOnLoad = function(onloadhandler)
{
 this.onload = onloadhandler;
 this.image.onload = function()
  {
   onloadhandler.onLoad();
  };
}

GlImage.prototype.setSrc = function(src_) { this.src = src_; this.image.src = src_; }

WebGl.prototype.createHTMLImageElement = function() {
	return new GlImage();
}

WebGl.prototype.createProgram = function() {
	return context.createProgram();
}

WebGl.prototype.createRenderbuffer = function() {
	return null;
}

WebGl.prototype.createShader = function(type) {
	return context.createShader(type);
}

WebGl.prototype.createTexture = function() {
	return context.createTexture();
}

WebGl.prototype.cullFace = function(mode) {
}

WebGl.prototype.deleteBuffer = function(buffer) {
}

WebGl.prototype.deleteFramebuffer = function(framebuffer) {
}

WebGl.prototype.deleteProgram = function(program) {
}

WebGl.prototype.deleteRenderbuffer = function(renderbuffer) {
}

WebGl.prototype.deleteShader = function(shader) {
}

WebGl.prototype.deleteTexture = function(texture) {
}

WebGl.prototype.depthFunc = function(func) {
}

WebGl.prototype.depthMask = function(flag) {
}

WebGl.prototype.depthRange = function(zNear, zFar) {
}

WebGl.prototype.detachShader = function(program, shader) {
}

WebGl.prototype.disable = function(cap) {
}

WebGl.prototype.disableVertexAttribArray = function(index) {
}

WebGl.prototype.drawArrays = function(mode, first, count) {
context.drawArrays(mode, first, count);
}

WebGl.prototype.drawElements = function(mode, count, type, offset) {
context.drawElements(mode, count, type, offset);
}

WebGl.prototype.drawingBufferHeight = function() {
	return 0;
}

WebGl.prototype.drawingBufferWidth = function() {
	return 0;
}

WebGl.prototype.enable = function(cap) {
context.enable(cap);
}

WebGl.prototype.enableVertexAttribArray = function(index) {
context.enableVertexAttribArray(index);
}

WebGl.prototype.finish = function() {
}

WebGl.prototype.flush = function() {
}

WebGl.prototype.framebufferRenderbuffer = function(target, attachment, renderbuffertarget, renderbuffer) {
}

WebGl.prototype.framebufferTexture2D = function(target, attachment, textarget, texture, level) {
}

WebGl.prototype.frontFace = function(mode) {
}

WebGl.prototype.generateMipmap = function(target) {
}

WebGl.prototype.getActiveAttrib = function(program, index) {
	return null;
}

WebGl.prototype.getActiveUniform = function(program, index) {
	return null;
}

WebGl.prototype.getAttachedShaders = function(program, outCount) {
	return null;
}

WebGl.prototype.getAttribLocation = function(program, name) {
	return context.getAttribLocation(program, name);
}

WebGl.prototype.getBufferParameter = function(target, pname) {
	return null;
}

WebGl.prototype.getContextAttributes = function() {
	return null;
}

WebGl.prototype.getError = function() {
	return 0;
}

WebGl.prototype.getExtension = function(name) {
	return null;
}

WebGl.prototype.getFramebufferAttachmentParameter = function(target, attachment, pname) {
	return null;
}

WebGl.prototype.getParameter = function(pname) {
	return null;
}

WebGl.prototype.getProgramInfoLog = function(program) {
	return null;
}

WebGl.prototype.getProgramParameter = function(program, pname) {
	return context.getProgramParameter(program, pname);
}

WebGl.prototype.getRenderbufferParameter = function(target, pname) {
	return null;
}

WebGl.prototype.getShaderInfoLog = function(shader) {
	return null;
}

WebGl.prototype.getShaderParameter = function(shader, pname) {
	return context.getShaderParameter(shader, pname);
}

WebGl.prototype.getShaderPrecisionFormat = function(shadertype, precisiontype) {
	return null;
}

WebGl.prototype.getShaderSource = function(shader) {
	return null;
}

WebGl.prototype.getSupportedExtensions = function(outCount) {
	return null;
}

WebGl.prototype.getTexParameter = function(target, pname) {
	return null;
}

WebGl.prototype.getUniform = function(program, location) {
	return null;
}

WebGl.prototype.getUniformLocation = function(program, name) {
	return context.getUniformLocation(program, name);
}

WebGl.prototype.getVertexAttrib = function(index, pname) {
	return null;
}

WebGl.prototype.getVertexAttribOffset = function(index, pname) {
	return 0;
}

WebGl.prototype.hint = function(target, mode) {
}

WebGl.prototype.isBuffer = function(buffer) {
	return false;
}

WebGl.prototype.isContextLost = function() {
	return false;
}

WebGl.prototype.isEnabled = function(cap) {
	return false;
}

WebGl.prototype.isFramebuffer = function(framebuffer) {
	return false;
}

WebGl.prototype.isProgram = function(program) {
	return false;
}

WebGl.prototype.isRenderbuffer = function(renderbuffer) {
	return false;
}

WebGl.prototype.isShader = function(shader) {
	return false;
}

WebGl.prototype.isTexture = function(texture) {
	return false;
}

WebGl.prototype.lineWidth = function(width) {
}

WebGl.prototype.linkProgram = function(program) {
context.linkProgram(program);
}

WebGl.prototype.pixelStorei = function(pname, param) {
context.pixelStorei(pname, param);
}

WebGl.prototype.polygonOffset = function(factor, units) {
}

WebGl.prototype.readPixels = function(x, y, width, height, format, type, pixels) {
}

WebGl.prototype.renderbufferStorage = function(target, internalformat, width, height) {
}

WebGl.prototype.sampleCoverage = function(value, invert) {
}

WebGl.prototype.scissor = function(x, y, width, height) {
}

WebGl.prototype.shaderSource = function(shader, source) {
context.shaderSource(shader, source);
}

WebGl.prototype.start = function() {
try {
canvas = document.getElementById("lesson01-canvas");
context = canvas.getContext("experimental-webgl");
} catch (e) {
}
if (!context) {
    alert("Could not initialise WebGL, sorry :-(");
}
tick();
}

var oldtime=0;
tick = function()
{
requestAnimationFrame(tick);
var now = performance.now();
var dt;
if(oldtime==0)
{
dt = 0;
}
else
{
dt = (now - oldtime) / 1000;
}
oldtime = now;
var args={};
args.getDt = function(){return dt;};
onnewframe.onNewFrame(args);
}

WebGl.prototype.stencilFunc = function(func, ref_, mask) {
}

WebGl.prototype.stencilFuncSeparate = function(face, func, ref_, mask) {
}

WebGl.prototype.stencilMask = function(mask) {
}

WebGl.prototype.stencilMaskSeparate = function(face, mask) {
}

WebGl.prototype.stencilOp = function(fail, zfail, zpass) {
}

WebGl.prototype.stencilOpSeparate = function(face, fail, zfail, zpass) {
}

WebGl.prototype.texImage2D = function(target, level, internalformat, width, height, border, format, type, pixels) {
context.texImage2D(target, level, internalformat, width, height, border, format, type, pixels);
}

WebGl.prototype.texImage2DImage = function(target, level, internalformat,
                    format, type, image) {
context.texImage2D(target, level, internalformat, format, type, image.image);
}

WebGl.prototype.texParameterf = function(target, pname, param) {
context.texParameterf(target, pname, param);
}

WebGl.prototype.texParameteri = function(target, pname, param) {
context.texParameteri(target, pname, param);
}

WebGl.prototype.texSubImage2D = function(target, level, xoffset, yoffset, width, height, format, type, pixels) {
}

WebGl.prototype.uniform1f = function(location, x) {
}

WebGl.prototype.uniform1fv = function(location, v) {
}

WebGl.prototype.uniform1i = function(location, x) {
context.uniform1i(location, x);
}

WebGl.prototype.uniform1iv = function(location, v) {
}

WebGl.prototype.uniform2f = function(location, x, y) {
}

WebGl.prototype.uniform2fv = function(location, v) {
}

WebGl.prototype.uniform2i = function(location, x, y) {
}

WebGl.prototype.uniform2iv = function(location, v) {
}

WebGl.prototype.uniform3f = function(location, x, y, z) {
}

WebGl.prototype.uniform3fv = function(location, v) {
}

WebGl.prototype.uniform3i = function(location, x, y, z) {
}

WebGl.prototype.uniform3iv = function(location, v) {
}

WebGl.prototype.uniform4fv = function(location, v) {
}

WebGl.prototype.uniform4i = function(location, x, y, z, w) {
}

WebGl.prototype.uniform4iv = function(location, v) {
}

WebGl.prototype.uniformMatrix2fv = function(location, transpose, value) {
}

WebGl.prototype.uniformMatrix3fv = function(location, transpose, value) {
}

WebGl.prototype.uniformMatrix4fv = function(location, transpose, value) {
context.uniformMatrix4fv(location, transpose, value);
}

WebGl.prototype.useProgram = function(program) {
context.useProgram(program);
}

WebGl.prototype.validateProgram = function(program) {
}

WebGl.prototype.vertexAttrib1f = function(indx, x) {
}

WebGl.prototype.vertexAttrib1fv = function(indx, values) {
}

WebGl.prototype.vertexAttrib2f = function(indx, x, y) {
}

WebGl.prototype.vertexAttrib2fv = function(indx, values) {
}

WebGl.prototype.vertexAttrib3f = function(indx, x, y, z) {
}

WebGl.prototype.vertexAttrib3fv = function(indx, values) {
}

WebGl.prototype.vertexAttrib4f = function(indx, x, y, z, w) {
}

WebGl.prototype.vertexAttrib4fv = function(indx, values) {
}

WebGl.prototype.vertexAttribPointer = function(indx, size, type, normalized, stride, offset) {
context.vertexAttribPointer(indx, size, type, normalized, stride, offset);
}

WebGl.prototype.viewport = function(x, y, width, height) {
context.viewport(x, y, width, height);
}

WebGl.prototype.getCanvasWidth = function() {
return canvas.width;
}

WebGl.prototype.getCanvasHeight = function() {
return canvas.height;
}