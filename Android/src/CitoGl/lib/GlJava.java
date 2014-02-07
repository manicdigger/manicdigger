package CitoGl.lib; import ManicDigger.lib.*;

public class GlJava extends Gl
{

    @Override
    public void activeTexture(int texture) {
        
    }

    @Override
    public void addOnKeyEvent(KeyEventHandler handler) {

    }

    @Override
    public void addOnMouseEvent(MouseEventHandler handler) {

    }

    @Override
    public void addOnNewFrame(NewFrameHandler handler) {

    }

    @Override
    public void addOnTouchEvent(TouchEventHandler handler) {

    }

    @Override
    public void attachShader(WebGLProgram program, WebGLShader shader) {

    }

    @Override
    public void bindAttribLocation(WebGLProgram program, int index, String name) {

    }

    @Override
    public void bindBuffer(int target, WebGLBuffer buffer) {

    }

    @Override
    public void bindFramebuffer(int target, WebGLFramebuffer framebuffer) {

    }

    @Override
    public void bindRenderbuffer(int target, WebGLRenderbuffer renderbuffer) {

    }

    @Override
    public void bindTexture(int target, WebGLTexture texture) {

    }

    @Override
    public void blendColor(float red, float green, float blue, float alpha) {

    }

    @Override
    public void blendEquation(int mode) {

    }

    @Override
    public void blendEquationSeparate(int modeRGB, int modeAlpha) {

    }

    @Override
    public void blendFunc(int sfactor, int dfactor) {

    }

    @Override
    public void blendFuncSeparate(int srcRGB, int dstRGB, int srcAlpha, int dstAlpha) {

    }

    @Override
    public void bufferData1(int target, int size, int usage) {

    }

    @Override
    public void bufferData2(int target, GlArrayBufferView data, int usage) {

    }

    @Override
    public void bufferData3(int target, GlArrayBuffer data, int usage) {

    }

    @Override
    public void bufferDataFloat(int target, float[] data, int usage) {

    }

    @Override
    public void bufferDataUshort(int target, int[] data, int usage) {

    }

    @Override
    public void bufferSubData1(int target, int offset, GlArrayBufferView data) {

    }

    @Override
    public void bufferSubData2(int target, int offset, GlArrayBuffer data) {

    }

    @Override
    public int checkFramebufferStatus(int target) {
        return 0;
    }

    @Override
    public void clear(int mask) {

    }

    @Override
    public void clearColor(float red, float green, float blue, float alpha) {

    }

    @Override
    public void clearDepth(float depth) {

    }

    @Override
    public void clearStencil(int s) {

    }

    @Override
    public void colorMask(boolean red, boolean green, boolean blue, boolean alpha) {

    }

    @Override
    public void compileShader(WebGLShader shader) {

    }

    @Override
    public void compressedTexImage2D(int target, int level, int internalformat, int width, int height, int border, GlArrayBufferView data) {

    }

    @Override
    public void compressedTexSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, GlArrayBufferView data) {

    }

    @Override
    public void copyTexImage2D(int target, int level, int internalformat, int x, int y, int width, int height, int border) {

    }

    @Override
    public void copyTexSubImage2D(int target, int level, int xoffset, int yoffset, int x, int y, int width, int height) {

    }

    @Override
    public WebGLBuffer createBuffer() {
        return null;
    }

    @Override
    public WebGLFramebuffer createFramebuffer() {
        return null;
    }

    @Override
    public HTMLImageElement createHTMLImageElement() {
        return null;
    }

    @Override
    public WebGLProgram createProgram() {
        return null;
    }

    @Override
    public WebGLRenderbuffer createRenderbuffer() {
        return null;
    }

    @Override
    public WebGLShader createShader(int type) {
        return null;
    }

    @Override
    public WebGLTexture createTexture() {
        return null;
    }

    @Override
    public void cullFace(int mode) {

    }

    @Override
    public void deleteBuffer(WebGLBuffer buffer) {

    }

    @Override
    public void deleteFramebuffer(WebGLFramebuffer framebuffer) {

    }

    @Override
    public void deleteProgram(WebGLProgram program) {

    }

    @Override
    public void deleteRenderbuffer(WebGLRenderbuffer renderbuffer) {

    }

    @Override
    public void deleteShader(WebGLShader shader) {

    }

    @Override
    public void deleteTexture(WebGLTexture texture) {

    }

    @Override
    public void depthFunc(int func) {

    }

    @Override
    public void depthMask(boolean flag) {

    }

    @Override
    public void depthRange(float zNear, float zFar) {

    }

    @Override
    public void detachShader(WebGLProgram program, WebGLShader shader) {

    }

    @Override
    public void disable(int cap) {

    }

    @Override
    public void disableVertexAttribArray(int index) {

    }

    @Override
    public void drawArrays(int mode, int first, int count) {

    }

    @Override
    public void drawElements(int mode, int count, int type, int offset) {

    }

    @Override
    public int drawingBufferHeight() {
        return 0;
    }

    @Override
    public int drawingBufferWidth() {
        return 0;
    }

    @Override
    public void enable(int cap) {

    }

    @Override
    public void enableVertexAttribArray(int index) {

    }

    @Override
    public void exitFullScreen() {

    }

    @Override
    public void exitPointerLock() {

    }

    @Override
    public void finish() {

    }

    @Override
    public void flush() {

    }

    @Override
    public void framebufferRenderbuffer(int target, int attachment, int renderbuffertarget, WebGLRenderbuffer renderbuffer) {

    }

    @Override
    public void framebufferTexture2D(int target, int attachment, int textarget, WebGLTexture texture, int level) {

    }

    @Override
    public void frontFace(int mode) {

    }

    @Override
    public void generateMipmap(int target) {

    }

    @Override
    public WebGLActiveInfo getActiveAttrib(WebGLProgram program, int index) {
        return null;
    }

    @Override
    public WebGLActiveInfo getActiveUniform(WebGLProgram program, int index) {
        return null;
    }

    @Override
    public WebGLShader[] getAttachedShaders(WebGLProgram program, Int outCount) {
        return new WebGLShader[0];
    }

    @Override
    public int getAttribLocation(WebGLProgram program, String name) {
        return 0;
    }

    @Override
    public GlObject getBufferParameter(int target, int pname) {
        return null;
    }

    @Override
    public int getCanvasHeight() {
        return 0;
    }

    @Override
    public int getCanvasWidth() {
        return 0;
    }

    @Override
    public WebGLContextAttributes getContextAttributes() {
        return null;
    }

    @Override
    public int getError() {
        return 0;
    }

    @Override
    public GlObject getExtension(String name) {
        return null;
    }

    @Override
    public GlObject getFramebufferAttachmentParameter(int target, int attachment, int pname) {
        return null;
    }

    @Override
    public GlObject getParameter(int pname) {
        return null;
    }

    @Override
    public String getProgramInfoLog(WebGLProgram program) {
        return null;
    }

    @Override
    public String getProgramParameter(WebGLProgram program, int pname) {
        return null;
    }

    @Override
    public GlObject getRenderbufferParameter(int target, int pname) {
        return null;
    }

    @Override
    public String getShaderInfoLog(WebGLShader shader) {
        return null;
    }

    @Override
    public GlObject getShaderParameter(WebGLShader shader, int pname) {
        return null;
    }

    @Override
    public WebGLShaderPrecisionFormat getShaderPrecisionFormat(int shadertype, int precisiontype) {
        return null;
    }

    @Override
    public String getShaderSource(WebGLShader shader) {
        return null;
    }

    @Override
    public String[] getSupportedExtensions(Int outCount) {
        return new String[0];
    }

    @Override
    public GlObject getTexParameter(int target, int pname) {
        return null;
    }

    @Override
    public GlObject getUniform(WebGLProgram program, WebGLUniformLocation location) {
        return null;
    }

    @Override
    public WebGLUniformLocation getUniformLocation(WebGLProgram program, String name) {
        return null;
    }

    @Override
    public GlObject getVertexAttrib(int index, int pname) {
        return null;
    }

    @Override
    public int getVertexAttribOffset(int index, int pname) {
        return 0;
    }

    @Override
    public void hint(int target, int mode) {

    }

    @Override
    public boolean isBuffer(WebGLBuffer buffer) {
        return false;
    }

    @Override
    public boolean isContextLost() {
        return false;
    }

    @Override
    public boolean isEnabled(int cap) {
        return false;
    }

    @Override
    public boolean isFramebuffer(WebGLFramebuffer framebuffer) {
        return false;
    }

    @Override
    public boolean isFullScreenEnabled() {
        return false;
    }

    @Override
    public boolean isPointerLockEnabled() {
        return false;
    }

    @Override
    public boolean isProgram(WebGLProgram program) {
        return false;
    }

    @Override
    public boolean isRenderbuffer(WebGLRenderbuffer renderbuffer) {
        return false;
    }

    @Override
    public boolean isShader(WebGLShader shader) {
        return false;
    }

    @Override
    public boolean isTexture(WebGLTexture texture) {
        return false;
    }

    @Override
    public void lineWidth(float width) {

    }

    @Override
    public void linkProgram(WebGLProgram program) {

    }

    @Override
    public void pixelStorei(int pname, int param) {

    }

    @Override
    public void polygonOffset(float factor, float units) {

    }

    @Override
    public void readPixels(int x, int y, int width, int height, int format, int type, GlArrayBufferView pixels) {

    }

    @Override
    public void renderbufferStorage(int target, int internalformat, int width, int height) {

    }

    @Override
    public void requestFullScreen() {

    }

    @Override
    public void requestPointerLock() {

    }

    @Override
    public void sampleCoverage(float value, boolean invert) {

    }

    @Override
    public void scissor(int x, int y, int width, int height) {

    }

    @Override
    public void shaderSource(WebGLShader shader, String source) {

    }

    @Override
    public void start() {

    }

    @Override
    public void stencilFunc(int func, int ref_, int mask) {

    }

    @Override
    public void stencilFuncSeparate(int face, int func, int ref_, int mask) {

    }

    @Override
    public void stencilMask(int mask) {

    }

    @Override
    public void stencilMaskSeparate(int face, int mask) {

    }

    @Override
    public void stencilOp(int fail, int zfail, int zpass) {

    }

    @Override
    public void stencilOpSeparate(int face, int fail, int zfail, int zpass) {

    }

    @Override
    public void texImage2D(int target, int level, int internalformat, int width, int height, int border, int format, int type, GlArrayBufferView pixels) {

    }

    @Override
    public void texImage2DCanvas(int target, int level, int internalformat, int format, int type, HTMLCanvasElement canvas) {

    }

    @Override
    public void texImage2DImage(int target, int level, int internalformat, int format, int type, HTMLImageElement image) {

    }

    @Override
    public void texImage2DImageData(int target, int level, int internalformat, int format, int type, ImageData pixels) {

    }

    @Override
    public void texImage2DVideo(int target, int level, int internalformat, int format, int type, HTMLVideoElement video) {

    }

    @Override
    public void texParameterf(int target, int pname, float param) {

    }

    @Override
    public void texParameteri(int target, int pname, int param) {

    }

    @Override
    public void texSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, int type, GlArrayBufferView pixels) {

    }

    @Override
    public void texSubImage2DCanvas(int target, int level, int xoffset, int yoffset, int format, int type, HTMLCanvasElement canvas) {

    }

    @Override
    public void texSubImage2DImage(int target, int level, int xoffset, int yoffset, int format, int type, HTMLImageElement image) {

    }

    @Override
    public void texSubImage2DImageData(int target, int level, int xoffset, int yoffset, int format, int type, ImageData pixels) {

    }

    @Override
    public void texSubImage2DVideo(int target, int level, int xoffset, int yoffset, int format, int type, HTMLVideoElement video) {

    }

    @Override
    public void uniform1f(WebGLUniformLocation location, float x) {

    }

    @Override
    public void uniform1fv(WebGLUniformLocation location, float[] v) {

    }

    @Override
    public void uniform1i(WebGLUniformLocation location, int x) {

    }

    @Override
    public void uniform1iv(WebGLUniformLocation location, int[] v) {

    }

    @Override
    public void uniform2f(WebGLUniformLocation location, float x, float y) {

    }

    @Override
    public void uniform2fv(WebGLUniformLocation location, float[] v) {

    }

    @Override
    public void uniform2i(WebGLUniformLocation location, int x, int y) {

    }

    @Override
    public void uniform2iv(WebGLUniformLocation location, int[] v) {

    }

    @Override
    public void uniform3f(WebGLUniformLocation location, float x, float y, float z) {

    }

    @Override
    public void uniform3fv(WebGLUniformLocation location, float[] v) {

    }

    @Override
    public void uniform3i(WebGLUniformLocation location, int x, int y, int z) {

    }

    @Override
    public void uniform3iv(WebGLUniformLocation location, int[] v) {

    }

    @Override
    public void uniform4fv(WebGLUniformLocation location, float[] v) {

    }

    @Override
    public void uniform4i(WebGLUniformLocation location, int x, int y, int z, int w) {

    }

    @Override
    public void uniform4iv(WebGLUniformLocation location, int[] v) {

    }

    @Override
    public void uniformMatrix2fv(WebGLUniformLocation location, boolean transpose, float[] value) {

    }

    @Override
    public void uniformMatrix3fv(WebGLUniformLocation location, boolean transpose, float[] value) {

    }

    @Override
    public void uniformMatrix4fv(WebGLUniformLocation location, boolean transpose, float[] value) {

    }

    @Override
    public void useProgram(WebGLProgram program) {

    }

    @Override
    public void validateProgram(WebGLProgram program) {

    }

    @Override
    public void vertexAttrib1f(int indx, float x) {

    }

    @Override
    public void vertexAttrib1fv(int indx, float[] values) {

    }

    @Override
    public void vertexAttrib2f(int indx, float x, float y) {

    }

    @Override
    public void vertexAttrib2fv(int indx, float[] values) {

    }

    @Override
    public void vertexAttrib3f(int indx, float x, float y, float z) {

    }

    @Override
    public void vertexAttrib3fv(int indx, float[] values) {

    }

    @Override
    public void vertexAttrib4f(int indx, float x, float y, float z, float w) {

    }

    @Override
    public void vertexAttrib4fv(int indx, float[] values) {

    }

    @Override
    public void vertexAttribPointer(int indx, int size, int type, boolean normalized, int stride, int offset) {

    }

    @Override
    public void viewport(int x, int y, int width, int height) {

    }
}