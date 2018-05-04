var canvas;

function PlatformJs() {}

PlatformJs.prototype = new GamePlatform();

PlatformJs.prototype.addOnCrash = function(handler) {};

var keyEventHandler;
PlatformJs.prototype.addOnKeyEvent = function(handler) {
	keyEventHandler = handler;
};

var mouseEventHandler;
PlatformJs.prototype.addOnMouseEvent = function(handler) {
	mouseEventHandler = handler;
};

var newFrameHandler;
PlatformJs.prototype.addOnNewFrame = function(handler) {
	newFrameHandler = handler;
};

var touchEventHandler;
PlatformJs.prototype.addOnTouchEvent = function(handler) {
	touchEventHandler = handler;
};

PlatformJs.prototype.applicationDoEvents = function() {};

// Audio
var audioContext;

PlatformJs.prototype.audioDataCreate = function(data, dataLength) {
	var sound = {};
	sound.loaded = false;
	var arr = new ArrayBuffer(dataLength);
	var arr8 = new Uint8Array(arr);
	for (var i = 0; i < dataLength; i++) {
		arr8[i] = data[i];
	}
	audioContext.decodeAudioData(arr, function(buffer) {
		sound.buffer = buffer;
		sound.loaded = true;
	});
	return sound;
};

PlatformJs.prototype.audioDataLoaded = function(data) {
	return data.loaded;
};

PlatformJs.prototype.audioCreate = function(data) {
	var audio = {};
	audio.data = data;
	audio.pausedAt = null;
	return audio;
};

PlatformJs.prototype.audioPlay = function(audio) {
	audio.src = audioContext.createBufferSource();
	audio.src.buffer = audio.data.buffer;
	audio.src.connect(audioContext.destination);
	if (audio.pausedAt) {
		audio.startedAt = Date.now() - audio.pausedAt;
		audio.src.start(0, audio.pausedAt / 1000);
		audio.pausedAt = null;
	} else {
		audio.startedAt = Date.now();
		audio.src.start(0);
	}
};

PlatformJs.prototype.audioPause = function(audio) {
	audio.src.stop();
	audio.pausedAt = Date.now() - audio.startedAt;
};

PlatformJs.prototype.audioDelete = function(audio) {
	audio.src.stop();
};

PlatformJs.prototype.audioFinished = function(audio) {
	return (Date.now() - audio.startedAt) > audio.data.buffer.duration * 1000;
};

PlatformJs.prototype.audioSetPosition = function(audio, x, y, z) {};

PlatformJs.prototype.audioUpdateListener = function(posX, posY, posZ, orientX, orientY, orientZ) {};

PlatformJs.prototype.aviWriterCreate = function() {
	return null;
};

PlatformJs.prototype.bindTexture2d = function(texture) {
	try {
		gl.bindTexture(gl.TEXTURE_2D, loadedTextures[texture]);
	} catch (ex) {
		console.log(ex);
	}
};

var textureCanvas;
var textureCanvasContext;

PlatformJs.prototype.bitmapCreate = function(width, height) {
	var c = document.createElement('canvas');
	c.width = width;
	c.height = height;
	c.loaded = true;
	return c;
};

var BASE64_CHUNK_SIZE = 100;
var toBase64 = function(data) {
	var str = "";
	for (var i = 0; i < data.length; i += BASE64_CHUNK_SIZE) {
		str += String.fromCharCode.apply(String, data.subarray(i, i + BASE64_CHUNK_SIZE));
	}
	return btoa(str);
};

PlatformJs.prototype.bitmapCreateFromPng = function(data, dataLength) {
	var c = document.createElement('canvas');
	if (!data) {
		return c;
	}
	// "data:image/png;base64" is buggy in Firefox.
	// Use custom png decoder instead.
	try {
		var png = new PNG(data);
		png.render(c);
	} catch (ex) {
		console.log(ex);
	}
	c.loaded = true;
	return c;
};

PlatformJs.prototype.bitmapDelete = function(bmp) {};

PlatformJs.prototype.bitmapGetHeight = function(bmp) {
	if (bmp == null) {
		return 100;
	}
	return bmp.height;
};

var colorA = function(color) {
	var a = ConvertCi.intToByte(color >> 24);
	return a;
};

var colorB = function(color) {
	var b = ConvertCi.intToByte(color);
	return b;
};

var colorG = function(color) {
	var g = ConvertCi.intToByte(color >> 8);
	return g;
};

var colorR = function(color) {
	var r = ConvertCi.intToByte(color >> 16);
	return r;
};

var colorFromArgb = function(a, r, g, b) {
	var iCol = a << 24 | r << 16 | g << 8 | b;
	return iCol;
};

PlatformJs.prototype.bitmapGetPixelsArgb = function(bitmap, bmpPixels) {
	var imageData = bitmap.getContext("2d").getImageData(0, 0, bitmap.width, bitmap.height);
	var width = bitmap.width;
	var height = bitmap.height;

	for (var x = 0; x < width; x++) {
		for (var y = 0; y < height; y++) {
			var pos = y * width + x;
			var r = imageData.data[pos * 4 + 0];
			var g = imageData.data[pos * 4 + 1];
			var b = imageData.data[pos * 4 + 2];
			var a = imageData.data[pos * 4 + 3];
			bmpPixels[pos] = colorFromArgb(a, r, g, b);
		}
	}
	bitmap.getContext("2d").putImageData(imageData, 0, 0);
};

PlatformJs.prototype.bitmapGetWidth = function(bmp) {
	if (bmp == null) {
		return 100;
	}
	return bmp.width;
};

PlatformJs.prototype.bitmapSetPixelsArgb = function(bmp, pixels) {
	var imageData = bmp.getContext("2d").createImageData(bmp.width, bmp.height);
	var arr = imageData.data;
	var width = bmp.width;
	var height = bmp.height;
	for (var x = 0; x < width; x++) {
		for (var y = 0; y < height; y++) {
			var pos = y * width + x;
			var color = pixels[pos];
			var r = colorR(color);
			var g = colorG(color);
			var b = colorB(color);
			var a = colorA(color);
			arr[pos * 4 + 0] = r;
			arr[pos * 4 + 1] = g;
			arr[pos * 4 + 2] = b;
			arr[pos * 4 + 3] = a;
		}
	}
	bmp.getContext("2d").putImageData(imageData, 0, 0);
};

PlatformJs.prototype.byteArrayLength = function(arr) {
	return arr.length;
};

PlatformJs.prototype.castToPlayerInterpolationState = function(a) {
	return a;
};

PlatformJs.prototype.changeResolution = function(width, height, bitsPerPixel, refreshRate) {};

PlatformJs.prototype.charArrayToString = function(charArray, length) {
	var arr = [];
	for (var i = 0; i < length; i++) {
		arr[i] = String.fromCharCode(charArray[i]);
	}
	return arr.join("");
};

PlatformJs.prototype.chatLog = function(servername, p) {
	return false;
};

PlatformJs.prototype.clipboardContainsText = function() {
	return false;
};

PlatformJs.prototype.clipboardGetText = function() {
	return null;
};

PlatformJs.prototype.clipboardSetText = function(s) {};

PlatformJs.prototype.consoleWriteLine = function(p) {
	console.log(p);
};

PlatformJs.prototype.createModel = function(modelData) {
	model = {};

	model.cubeVertexPositionBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexPositionBuffer);
	var vertices = modelData.xyz;
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
	model.cubeVertexPositionBuffer.itemSize = 3;
	model.cubeVertexPositionBuffer.numItems = modelData.verticesCount;

	model.cubeVertexTextureCoordBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexTextureCoordBuffer);
	var textureCoords = modelData.uv;
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(textureCoords), gl.STATIC_DRAW);
	model.cubeVertexTextureCoordBuffer.itemSize = 2;
	model.cubeVertexTextureCoordBuffer.numItems = modelData.verticesCount;

	model.cubeVertexColorBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexColorBuffer);
	var colors2 = new Float32Array(modelData.verticesCount * 4);
	var colors = modelData.rgba;
	if (!colors) {
		for (var i = 0; i < colors2.length; i++) {
			colors2[i] = 1;
		}
	} else {
		for (var i = 0; i < colors.length; i++) {
			colors2[i] = colors[i] / 255;
		}
	}
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors2), gl.STATIC_DRAW);
	model.cubeVertexColorBuffer.itemSize = 4;
	model.cubeVertexColorBuffer.numItems = modelData.verticesCount;

	model.cubeVertexIndexBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, model.cubeVertexIndexBuffer);
	var cubeVertexIndices = modelData.indices;
	gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(cubeVertexIndices), gl.STATIC_DRAW);
	model.cubeVertexIndexBuffer.itemSize = 1;
	model.cubeVertexIndexBuffer.numItems = modelData.indicesCount;

	if (modelData.getMode() == DrawModeEnum.LINES) {
		model.mode = gl.LINES;
	} else {
		model.mode = gl.TRIANGLES;
	}

	return model;
};


function getPowerOfTwo(value, pow) {
	var pow = pow || 1;
	while (pow < value) {
		pow *= 2;
	}
	return pow;
}

function setFont(ctx, text, fontSize, color) {
	ctx.fillStyle = 'rgb(' + colorR(color) + ',' + colorG(color) + ',' + colorB(color) + ')';
	ctx.lineWidth = 3.5;
	ctx.strokeStyle = 'black';
	ctx.font = fontSize + "px Verdana";
	ctx.textAlign = '';
	ctx.textBaseline = 'top';
}

function cloneCanvas(oldCanvas) {

	//create a new canvas
	var newCanvas = document.createElement('canvas');
	var context = newCanvas.getContext('2d');

	//set dimensions
	newCanvas.width = oldCanvas.width;
	newCanvas.height = oldCanvas.height;

	//apply the old canvas to the new one
	context.drawImage(oldCanvas, 0, 0);

	//return the new canvas
	return newCanvas;
}

PlatformJs.prototype.createTextTexture = function(t) {
	textureCanvas.width = getPowerOfTwo(textureCanvasContext.measureText(t.text).width);
	textureCanvas.height = getPowerOfTwo(2 * t.font.size);

	setFont(textureCanvasContext, t.text, t.font.size, t.color);
	textureCanvasContext.fillText(t.text, 0, 0);

	return cloneCanvas(textureCanvas);
};

PlatformJs.prototype.deleteModel = function(model) {};

PlatformJs.prototype.directoryGetFiles = function(path, length) {
	return null;
};

PlatformJs.prototype.drawModel = function(model) {
	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexPositionBuffer);
	gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, model.cubeVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);

	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexTextureCoordBuffer);
	gl.vertexAttribPointer(shaderProgram.textureCoordAttribute, model.cubeVertexTextureCoordBuffer.itemSize, gl.FLOAT, false, 0, 0);

	gl.bindBuffer(gl.ARRAY_BUFFER, model.cubeVertexColorBuffer);
	gl.vertexAttribPointer(shaderProgram.vertexColorAttribute, model.cubeVertexColorBuffer.itemSize, gl.FLOAT, false, 0, 0);


	gl.activeTexture(gl.TEXTURE0);
	//gl.bindTexture(gl.TEXTURE_2D, crateTextures[filter]);
	gl.uniform1i(shaderProgram.samplerUniform, 0);

	gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, model.cubeVertexIndexBuffer);
	//setMatrixUniforms();
	gl.drawElements(model.mode, model.cubeVertexIndexBuffer.numItems, gl.UNSIGNED_SHORT, 0);
};

PlatformJs.prototype.drawModelData = function(data) {
	var model = this.createModel(data);
	this.drawModel(model);
	this.deleteModel(model);
};

PlatformJs.prototype.drawModels = function(model, count) {
	for (var i = 0; i < count; i++) {
		this.drawModel(model[i]);
	}
};

PlatformJs.prototype.enetAvailable = function() {
	return false;
};

PlatformJs.prototype.enetCreateHost = function() {
	return null;
};

PlatformJs.prototype.enetHostCheckEvents = function(host, event_) {
	return false;
};

PlatformJs.prototype.enetHostConnect = function(host, hostName, port, data, channelLimit) {
	return null;
};

PlatformJs.prototype.enetHostInitialize = function(host, address, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth) {};

PlatformJs.prototype.enetHostService = function(host, timeout, enetEvent) {
	return false;
};

PlatformJs.prototype.enetPeerSend = function(peer, channelID, data, dataLength, flags) {};

PlatformJs.prototype.exit = function() {};

PlatformJs.prototype.exitAvailable = function() {
	return false;
};

document.exitPointerLock = document.exitPointerLock ||
	document.mozExitPointerLock ||
	document.webkitExitPointerLock;
PlatformJs.prototype.exitMousePointerLock = function() {
	document.exitPointerLock();
};

PlatformJs.prototype.fileName = function(fullpath) {
	return null;
};

PlatformJs.prototype.fileOpenDialog = function(extension, extensionName, initialDirectory) {
	return null;
};

PlatformJs.prototype.fileReadAllLines = function(path, length) {
	return null;
};

PlatformJs.prototype.floatModulo = function(a, b) {
	return a % b;
};

PlatformJs.prototype.floatParse = function(value) {
	return parseFloat(value);
};

PlatformJs.prototype.floatToInt = function(value) {
	return value | 0;
};

PlatformJs.prototype.floatToString = function(value) {
	return value.toString();
};

PlatformJs.prototype.floatTryParse = function(s, ret) {
	if (!isNaN(s)) {
		ret.value = parseFloat(s);
		return true;
	}
	return false;
};

PlatformJs.prototype.focused = function() {
	return true;
};

PlatformJs.prototype.gLDeleteTexture = function(id) {};

PlatformJs.prototype.gLDisableAlphaTest = function() {};

PlatformJs.prototype.gLEnableAlphaTest = function() {};

PlatformJs.prototype.gLLineWidth = function(width) {};

PlatformJs.prototype.getCanvasHeight = function() {
	return canvas.height;
};

PlatformJs.prototype.getCanvasWidth = function() {
	return canvas.width;
};

PlatformJs.prototype.getDisplayResolutionDefault = function() {
	return null;
};

PlatformJs.prototype.getDisplayResolutions = function(resolutionsCount) {
	return null;
};

PlatformJs.prototype.getGameVersion = function() {
	return null;
};

PlatformJs.prototype.getLanguageIso6391 = function() {
	return "en";
};

PlatformJs.prototype.getPreferences = function() {
	var p = new Preferences();
	var pp = JSON.parse(localStorage.getItem("Preferences"));
	if (pp) {
		p.items.items = pp.items;
		p.items.count = pp.count;
	}
	return p;
};

PlatformJs.prototype.getWindowState = function() {
	return WindowState.MAXIMIZED;
};

PlatformJs.prototype.glClearColorBufferAndDepthBuffer = function() {
	gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
};

PlatformJs.prototype.glClearColorRgbaf = function(r, g, b, a) {
	gl.clearColor(r, g, b, a);
};

PlatformJs.prototype.glClearDepthBuffer = function() {
	gl.clear(gl.DEPTH_BUFFER_BIT);
};

PlatformJs.prototype.glColorMaterialFrontAndBackAmbientAndDiffuse = function() {};

PlatformJs.prototype.glCullFaceBack = function() {};

PlatformJs.prototype.glDepthMask = function(flag) {
	gl.depthMask(flag);
};

PlatformJs.prototype.glDisableCullFace = function() {};

PlatformJs.prototype.glDisableDepthTest = function() {
	gl.disable(gl.DEPTH_TEST);
};

PlatformJs.prototype.glDisableFog = function() {};

PlatformJs.prototype.glEnableColorMaterial = function() {};

PlatformJs.prototype.glEnableCullFace = function() {};

PlatformJs.prototype.glEnableDepthTest = function() {
	gl.enable(gl.DEPTH_TEST);
};

PlatformJs.prototype.glEnableFog = function() {};

PlatformJs.prototype.glEnableLighting = function() {};

PlatformJs.prototype.glEnableTexture2d = function() {};

PlatformJs.prototype.glDisableTexture2d = function() {};

PlatformJs.prototype.glFogFogColor = function(r, g, b, a) {};

PlatformJs.prototype.glFogFogDensity = function(density) {};

PlatformJs.prototype.glFogFogModeExp2 = function() {};

PlatformJs.prototype.glGetMaxTextureSize = function() {
	return 0;
};

PlatformJs.prototype.glHintFogHintNicest = function() {};

PlatformJs.prototype.glLightModelAmbient = function(r, g, b) {};

PlatformJs.prototype.glShadeModelSmooth = function() {};

PlatformJs.prototype.glViewport = function(x, y, width, height) {
	gl.viewport(x, y, width, height);
};

PlatformJs.prototype.glActiveTexture = function(textureUnit) {
	switch (textureUnit) {
		case 0:
			gl.activeTexture(gl.TEXTURE0);
			break;
		case 1:
			gl.activeTexture(gl.TEXTURE1);
			break;
		case 2:
			gl.activeTexture(gl.TEXTURE2);
			break;
		case 3:
			gl.activeTexture(gl.TEXTURE3);
			break;
	}
};

PlatformJs.prototype.glCreateProgram = function() {
	return gl.createProgram();
};

PlatformJs.prototype.glDeleteProgram = function(program) {
	gl.deleteProgram(program);
};

PlatformJs.prototype.glCreateShader = function(shaderType) {
	var glShaderType;
	switch (shaderType) {
		case 0:
			glShaderType = gl.VERTEX_SHADER;
			break;
		case 1:
		default:
			glShaderType = gl.FRAGMENT_SHADER;
			break;
	}
	return gl.createShader(glShaderType);
};

PlatformJs.prototype.glShaderSource = function(shader, source) {
	gl.shaderSource(shader, source);
};

PlatformJs.prototype.glCompileShader = function(shader) {
	gl.compileShader(shader);
};

PlatformJs.prototype.glGetShaderCompileStatus = function(shader) {
	return gl.getShaderParameter(shader, gl.COMPILE_STATUS);
};

PlatformJs.prototype.glGetShaderInfoLog = function(shader) {
	return gl.getShaderInfoLog(shader);
};

PlatformJs.prototype.glAttachShader = function(program, shader) {
	gl.attachShader(program, shader);
};

PlatformJs.prototype.glUseProgram = function(program) {
	gl.useProgram(program);
};

PlatformJs.prototype.glGetUniformLocation = function(program, name) {
	return gl.getUniformLocation(program, name);
};

PlatformJs.prototype.glLinkProgram = function(program) {
	gl.linkProgram(program);
};

PlatformJs.prototype.glGetProgramLinkStatus = function(program) {
	return gl.getProgramParameter(program, gl.LINK_STATUS);
};

PlatformJs.prototype.glGetProgramInfoLog = function(program) {
	return gl.getProgramInfoLog(program);
};

PlatformJs.prototype.glGetStringSupportedShadingLanguage = function() {
	return gl.getParameter(gl.SHADING_LANGUAGE_VERSION);
};

PlatformJs.prototype.glUniform1i = function(location, v0) {
	gl.uniform1i(location, v0);
};

PlatformJs.prototype.glUniform1f = function(location, v0) {
	gl.uniform1f(location, v0);
};

PlatformJs.prototype.glUniform2f = function(location, v0, v1) {
	gl.uniform2f(location, v0, v1);
};

PlatformJs.prototype.glUniform3f = function(location, v0, v1, v2) {
	gl.uniform3f(location, v0, v1, v2);
};

PlatformJs.prototype.glUniform4f = function(location, v0, v1, v2, v3) {
	gl.uniform4f(location, v0, v1, v2, v3);
};

PlatformJs.prototype.glUniformArray1f = function(location, count, values) {
	gl.uniform2fv(location, values);
};

PlatformJs.prototype.grabScreenshot = function() {
	return null;
};

PlatformJs.prototype.gzipCompress = function(data, dataLength, retLength) {
	return null;
};

PlatformJs.prototype.gzipDecompress = function(compressed, compressedLength, ret) {
	var compressed2 = [];
	for (var i = 0; i < compressedLength; i++) {
		compressed2[i] = compressed[i];
	}

	var gunzip = new Zlib.Gunzip(compressed2);
	var decompressed = gunzip.decompress();

	for (var i = 0; i < decompressed.length; i++) {
		ret[i] = decompressed[i];
	}
};

PlatformJs.prototype.initShaders = function() {};

PlatformJs.prototype.intParse = function(value) {
	return parseInt(value);
};

PlatformJs.prototype.intTryParse = function(s, ret) {
	if (!isNaN(s)) {
		ret.value = parseInt(s);
		return true;
	}
	return false;
};

PlatformJs.prototype.intToString = function(value) {
	return value.toString();
};

PlatformJs.prototype.isCached = function(md5) {
	return false;
};

PlatformJs.prototype.isChecksum = function(checksum) {
	return false;
};

PlatformJs.prototype.decodeHTMLEntities = function(htmlencodedstring) {
	var parser = new DOMParser();
	var dom = parser.parseFromString('<body>' + htmlencodedstring, 'text/html');
	return dom.body.textContent;
};

PlatformJs.prototype.isDebuggerAttached = function() {
	return false;
};

PlatformJs.prototype.isFastSystem = function() {
	return false;
};

function _isMousePointerLocked() {
	return document.pointerLockElement != null || document.mozPointerLockElement != null;
}

PlatformJs.prototype.isMousePointerLocked = function() {
	return _isMousePointerLocked();
};

PlatformJs.prototype.isSmallScreen = function() {
	return true;
};

PlatformJs.prototype.isValidTypingChar = function(c) {
	return c != 13 && c != 8;
};

PlatformJs.prototype.keyName = function(key) {
	return null;
};

PlatformJs.prototype.loadAssetFromCache = function(md5) {
	return null;
};

//this function will work cross-browser for loading scripts asynchronously
function loadScript(src, callback) {
	var s,
		r,
		t;
	r = false;
	s = document.createElement('script');
	s.type = 'text/javascript';
	s.src = src;
	s.onload = s.onreadystatechange = function() {
		//console.log( this.readyState ); //uncomment this line to see which ready states are called.
		if (!r && (!this.readyState || this.readyState == 'complete')) {
			r = true;
			callback();
		}
	};
	t = document.getElementsByTagName('script')[0];
	t.parentNode.insertBefore(s, t);
}

var assets;
PlatformJs.prototype.loadAssetsAsyc = function(list, progress) {

	loadScript("js/Assets.js", function() {
		assets = new Assets();
		list.count = assets.count;
		list.items = {};
		for (var i = 0; i < assets.count; i++) {
			var item = {};
			item.name = assets.name[i].toLowerCase();
			item.data = assets.data[i];
			item.dataLength = assets.length[i];
			item.md5 = CryptoJS.MD5(CryptoJS.lib.WordArray.create(assets.data[i])).toString(CryptoJS.enc.Hex);
			list.items[i] = item;
		}
		progress.value = 1;
	});
};

var loadedTextures = [];
loadedTextures[0] = null;

PlatformJs.prototype.loadTextureFromBitmap = function(bmp) {
	var texture = gl.createTexture();

	gl.bindTexture(gl.TEXTURE_2D, texture);
	gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, bmp);
	gl.generateMipmap(gl.TEXTURE_2D);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST_MIPMAP_LINEAR);

	gl.bindTexture(gl.TEXTURE_2D, null);

	loadedTextures.push(texture);
	return loadedTextures.length - 1;
};

PlatformJs.prototype.mathAcos = function(p) {
	return Math.acos(p);
};

PlatformJs.prototype.mathCos = function(a) {
	return Math.cos(a);
};

PlatformJs.prototype.mathSin = function(a) {
	return Math.sin(a);
};

PlatformJs.prototype.mathSqrt = function(value) {
	return Math.sqrt(value);
};

PlatformJs.prototype.mathTan = function(p) {
	return Math.tan(p);
};

PlatformJs.prototype.messageBoxShowError = function(text, caption) {
	alert(caption + "\n" + text);
};

PlatformJs.prototype.monitorCreate = function() {
	return null;
};

PlatformJs.prototype.monitorEnter = function(monitorObject) {};

PlatformJs.prototype.monitorExit = function(monitorObject) {};

PlatformJs.prototype.mouseCursorIsVisible = function() {
	return true;
};

PlatformJs.prototype.mouseCursorSetVisible = function(value) {};

PlatformJs.prototype.multithreadingAvailable = function() {
	return false;
};

PlatformJs.prototype.openLinkInBrowser = function(url) {};

PlatformJs.prototype.parseUri = function(uri) {
	return null;
};

PlatformJs.prototype.pathCombine = function(part1, part2) {
	return null;
};

PlatformJs.prototype.pathSavegames = function() {
	return null;
};

PlatformJs.prototype.pathStorage = function() {
	return null;
};

PlatformJs.prototype.queryStringValue = function(key) {
	// http://stackoverflow.com/a/3855394
	var qs = (function(a) {
		if (a == "") return {};
		var b = {};
		for (var i = 0; i < a.length; ++i) {
			var p = a[i].split('=', 2);
			if (p.length == 1)
				b[p[0]] = "";
			else
				b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
		}
		return b;
	})(window.location.search.substr(1).split('&'));
	return qs[key];
};

PlatformJs.prototype.queueUserWorkItem = function(action) {};

PlatformJs.prototype.randomCreate = function() {
	var random = {};
	random.next = function() {
		return Math.floor(Math.random() * 2147483647);
	};
	random.maxNext = function(max) {
		return Math.floor(Math.random() * max);
	};
	random.nextFloat = function() {
		return Math.random();
	};
	return random;
};

PlatformJs.prototype.readAllLines = function(p, retCount) {
	var lines = p.split("\n");
	retCount.value = lines.length;
	return lines;
};

PlatformJs.prototype.requestMousePointerLock = function() {
	canvas.requestPointerLock = canvas.requestPointerLock ||
		canvas.mozRequestPointerLock ||
		canvas.webkitRequestPointerLock;
	canvas.requestPointerLock();
};

PlatformJs.prototype.restoreWindowCursor = function() {
	document.getElementById("main").style.cursor = "auto";
};

PlatformJs.prototype.setWindowCursor = function(hotx, hoty, sizex, sizey, imgdata, imgdatalength) {
	document.getElementById("main").style.cursor = "url(data:image/png;base64," + toBase64(imgdata) + "), auto";
};

PlatformJs.prototype.saveAssetToCache = function(tosave) {};

// http://stackoverflow.com/a/18480879
function download(canvas, filename) {

	/// create an "off-screen" anchor tag
	var lnk = document.createElement('a'),
		e;

	/// the key here is to set the download attribute of the a tag
	lnk.download = filename;

	/// convert canvas content to data-uri for link. When download
	/// attribute is set the content pointed to by link will be
	/// pushed as "download" in HTML5 capable browsers
	lnk.href = canvas.toDataURL();

	/// create a "fake" click-event to trigger the download
	if (document.createEvent) {

		e = document.createEvent("MouseEvents");
		e.initMouseEvent("click", true, true, window,
			0, 0, 0, 0, 0, false, false, false,
			false, 0, null);

		lnk.dispatchEvent(e);

	} else if (lnk.fireEvent) {

		lnk.fireEvent("onclick");
	}
}

// http://stackoverflow.com/a/10632399
Number.prototype.padLeft = function(base, chr) {
	var len = (String(base || 10).length - String(this).length) + 1;
	return len > 0 ? new Array(len).join(chr || '0') + this : this;
};
PlatformJs.prototype.saveScreenshot = function() {
	var d = new Date,
		dformat = [d.getFullYear(),
			(d.getMonth() + 1).padLeft(),
			d.getDate().padLeft()
		].join('-') + '_' + [d.getHours().padLeft(),
			d.getMinutes().padLeft(),
			d.getSeconds().padLeft()
		].join('-');
	download(canvas, "Screenshot" + dformat + ".png");
};

PlatformJs.prototype.setMatrixUniformModelView = function(mvMatrix) {
	gl.uniformMatrix4fv(shaderProgram.mvMatrixUniform, false, mvMatrix);
};

PlatformJs.prototype.setMatrixUniformProjection = function(pMatrix) {
	gl.uniformMatrix4fv(shaderProgram.pMatrixUniform, false, pMatrix);
};

PlatformJs.prototype.setPreferences = function(preferences) {
	localStorage.setItem("Preferences", JSON.stringify(preferences.items));
};

PlatformJs.prototype.setTextRendererFont = function(fontID) {};

PlatformJs.prototype.setTitle = function(applicationname) {};

PlatformJs.prototype.setVSync = function(enabled) {};

PlatformJs.prototype.setWindowState = function(value) {};

PlatformJs.prototype.showKeyboard = function(show) {};

PlatformJs.prototype.singlePlayerServerAvailable = function() {
	return false;
};

PlatformJs.prototype.singlePlayerServerDisable = function() {};

PlatformJs.prototype.singlePlayerServerExit = function() {};

PlatformJs.prototype.singlePlayerServerGetNetwork = function() {
	return null;
};

PlatformJs.prototype.singlePlayerServerLoaded = function() {
	return false;
};

PlatformJs.prototype.singlePlayerServerStart = function(saveFilename) {};

PlatformJs.prototype.languageNativeAvailable = function() {
	return false;
};

PlatformJs.prototype.getLanguageHandler = function() {
	return new LanguageCi();
};

PlatformJs.prototype.stringContains = function(a, b) {
	return a.indexOf(b) != -1;
};

PlatformJs.prototype.stringEmpty = function(data) {
	return data == "";
};

var stringFormat = function(s, args) {
	var i = args.length;

	while (i--) {
		s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), args[i]);
	}
	return s;
};


PlatformJs.prototype.stringFormat = function(format, arg0) {
	return stringFormat(format, [arg0]);
};

PlatformJs.prototype.stringFormat2 = function(format, arg0, arg1) {
	return stringFormat(format, [arg0, arg1]);
};

PlatformJs.prototype.stringFormat3 = function(format, arg0, arg1, arg2) {
	return stringFormat(format, [arg0, arg1, arg2]);
};

PlatformJs.prototype.stringFormat4 = function(format, arg0, arg1, arg2, arg3) {
	return stringFormat(format, [arg0, arg1, arg2, arg3]);
};

PlatformJs.prototype.stringFromUtf8ByteArray = function(value, valueLength) {
	var arr = new Uint8Array(valueLength);
	for (var i = 0; i < valueLength; i++) {
		arr[i] = value[i];
	}
	var encodedString = String.fromCharCode.apply(null, arr),
		decodedString = decodeURIComponent(escape(encodedString));
	return decodedString;
};

PlatformJs.prototype.stringIndexOf = function(s, p) {
	return s.indexOf(p);
};

PlatformJs.prototype.stringReplace = function(s, from, to) {
	return s.replace(from, to);
};

PlatformJs.prototype.stringSplit = function(value, separator, returnLength) {
	var ret = value.split(separator);
	returnLength.value = ret.length;
	return ret;
};

PlatformJs.prototype.stringJoin = function(value, separator) {
	return value.join(separator);
};

PlatformJs.prototype.stringStartsWithIgnoreCase = function(a, b) {
	return a.toLowerCase().indexOf(b.toLowerCase()) == 0;
};

PlatformJs.prototype.stringToCharArray = function(s, length) {
	var ret = {};
	if (s == null) {
		s = "";
	}
	for (var i = 0; i < s.length; i++) {
		ret[i] = s.charCodeAt(i);
	}
	length.value = s.length;
	return ret;
};

PlatformJs.prototype.stringToLower = function(p) {
	return p.toLowerCase();
};

// http://stackoverflow.com/a/18729931
PlatformJs.prototype.stringToUtf8ByteArray = function(s, retLength) {
	var str = s;
	var utf8 = [];
	for (var i = 0; i < str.length; i++) {
		var charcode = str.charCodeAt(i);
		if (charcode < 0x80) utf8.push(charcode);
		else if (charcode < 0x800) {
			utf8.push(0xc0 | (charcode >> 6),
				0x80 | (charcode & 0x3f));
		} else if (charcode < 0xd800 || charcode >= 0xe000) {
			utf8.push(0xe0 | (charcode >> 12),
				0x80 | ((charcode >> 6) & 0x3f),
				0x80 | (charcode & 0x3f));
		}
		// surrogate pair
		else {
			i++;
			// UTF-16 encodes 0x10000-0x10FFFF by
			// subtracting 0x10000 and splitting the
			// 20 bits of 0x0-0xFFFFF into two halves
			charcode = 0x10000 + (((charcode & 0x3ff) << 10) |
				(str.charCodeAt(i) & 0x3ff));
			utf8.push(0xf0 | (charcode >> 18),
				0x80 | ((charcode >> 12) & 0x3f),
				0x80 | ((charcode >> 6) & 0x3f),
				0x80 | (charcode & 0x3f));
		}
	}
	retLength.value = utf8.length;
	return utf8;
};

PlatformJs.prototype.stringTrim = function(value) {
	return value.trim();
};

PlatformJs.prototype.tcpAvailable = function() {
	return false;
};

PlatformJs.prototype.tcpConnect = function(ip, port, connected) {};

PlatformJs.prototype.tcpReceive = function(data, dataLength) {
	return 0;
};

PlatformJs.prototype.tcpSend = function(data, length) {};

PlatformJs.prototype.textSize = function(text, font, outWidth, outHeight) {
	var canvas1 = document.getElementById('textureCanvas');
	var ctx = canvas1.getContext('2d');
	setFont(ctx, text, font.size, 0);
	outWidth.value = ctx.measureText(text).width;
	// workaround for height measurement as found on https://stackoverflow.com/a/13318387
	outHeight.value = ctx.measureText('M').width;
};

PlatformJs.prototype.threadSpinWait = function(iterations) {};

PlatformJs.prototype.throwException = function(message) {
	throw message;
};

PlatformJs.prototype.thumbnailDownloadAsync = function(ip, port, response) {};

if (window.performance.now) {
	console.log("Using high performance timer");
	getTimestamp = function() {
		return window.performance.now();
	};
} else {
	if (window.performance.webkitNow) {
		console.log("Using webkit high performance timer");
		getTimestamp = function() {
			return window.performance.webkitNow();
		};
	} else {
		console.log("Using low performance timer");
		getTimestamp = function() {
			return new Date().getTime();
		};
	}
}

var startTime = getTimestamp();

PlatformJs.prototype.timeMillisecondsFromStart = function() {
	return getTimestamp() - startTime;
};

PlatformJs.prototype.timestamp = function() {
	return null;
};

PlatformJs.prototype.webClientDownloadDataAsync = function(url, response) {
	var xhr = new XMLHttpRequest();
	xhr.open("GET", url);
	xhr.overrideMimeType('text/plain; charset=x-user-defined');
	xhr.onload = function() {
		if (this.status == 200) {
			var ret = {};
			for (var i = 0; i < xhr.response.length; i++) {
				ret[i] = xhr.response.charCodeAt(i) & 0xff;
			}
			response.value = ret;
			response.valueLength = xhr.response.length;
			response.done = true;
		}
	};
	xhr.onerror = function() {
		response.error = true;
	};
	xhr.send();
};

PlatformJs.prototype.webClientUploadDataAsync = function(url, data, dataLength, response) {
	var xhr = new XMLHttpRequest();
	xhr.open("POST", url);

	xhr.overrideMimeType('text/plain; charset=x-user-defined');
	xhr.onload = function() {
		if (this.status == 200) {
			var ret = {};
			for (var i = 0; i < xhr.response.length; i++) {
				ret[i] = xhr.response.charCodeAt(i) & 0xff;
			}
			response.value = ret;
			response.valueLength = xhr.response.length;
			response.done = true;
		}
	};
	var data2 = new Uint8Array(dataLength);
	for (var i = 0; i < dataLength; i++) {
		data2[i] = data[i];
	}
	xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
	xhr.send(data2);
};

PlatformJs.prototype.webSocketAvailable = function() {
	return true;
};

var websocket;
PlatformJs.prototype.webSocketConnect = function(ip, port) {
	websocket = new WebSocket("ws://" + ip + ":" + port + "/Game");
	websocket.binaryType = "arraybuffer";
	websocket.onopen = function(evt) {
		onOpen(evt);
	};
	websocket.onclose = function(evt) {
		onClose(evt);
	};
	websocket.onmessage = function(evt) {
		onMessage(evt);
	};
	websocket.onerror = function(evt) {
		onError(evt);
	};
};

var incoming = [];

var connected = false;

function onOpen(evt) {
	connected = true;
}

function onClose(evt) {}

function onMessage(evt) {
	incoming.push(evt.data);
}

function onError(evt) {}

var outgoing = [];

PlatformJs.prototype.webSocketSend = function(data, dataLength) {
	var data2 = new Uint8Array(dataLength);
	for (var i = 0; i < dataLength; i++) {
		data2[i] = data[i];
	}
	if (connected) {
		websocket.send(data2);
	} else {
		outgoing.push(data2);
	}
};

PlatformJs.prototype.webSocketReceive = function(data, dataLength) {
	if (connected) {
		while (outgoing.length != 0) {
			websocket.send(outgoing.shift());
		}
	}
	if (incoming.length != 0) {
		var packet = new Uint8Array(incoming.shift());
		for (var i = 0; i < packet.length; i++) {
			data[i] = packet[i];
		}
		return packet.length;
	} else {
		return -1;
	}
};

PlatformJs.prototype.windowExit = function() {};


PlatformJs.prototype.start = function() {};




/**
 * Provides requestAnimationFrame in a cross browser way.
 */
window.requestAnimFrame = (function() {
	return window.requestAnimationFrame ||
		window.webkitRequestAnimationFrame ||
		window.mozRequestAnimationFrame ||
		window.oRequestAnimationFrame ||
		window.msRequestAnimationFrame ||
		function( /* function FrameRequestCallback */ callback, /* DOMElement Element */ element) {
			window.setTimeout(callback, 1000 / 60);
		};
})();

var gl;

function initGL(canvas) {
	try {
		gl = canvas.getContext("experimental-webgl");
		gl.viewportWidth = canvas.width;
		gl.viewportHeight = canvas.height;
	} catch (e) {}
	if (!gl) {
		alert("Could not initialise WebGL, sorry :-(");
	}
}


function getShader(gl, id) {
	var shaderScript = document.getElementById(id);
	if (!shaderScript) {
		return null;
	}

	var str = "";
	var k = shaderScript.firstChild;
	while (k) {
		if (k.nodeType == 3) {
			str += k.textContent;
		}
		k = k.nextSibling;
	}

	var shader;
	if (shaderScript.type == "x-shader/x-fragment") {
		shader = gl.createShader(gl.FRAGMENT_SHADER);
	} else if (shaderScript.type == "x-shader/x-vertex") {
		shader = gl.createShader(gl.VERTEX_SHADER);
	} else {
		return null;
	}

	gl.shaderSource(shader, str);
	gl.compileShader(shader);

	if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
		alert(gl.getShaderInfoLog(shader));
		return null;
	}

	return shader;
}


var shaderProgram;

function initShaders() {
	var fragmentShader = getShader(gl, "shader-fs");
	var vertexShader = getShader(gl, "shader-vs");

	shaderProgram = gl.createProgram();
	gl.attachShader(shaderProgram, vertexShader);
	gl.attachShader(shaderProgram, fragmentShader);
	gl.linkProgram(shaderProgram);

	if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
		alert("Could not initialise shaders");
	}

	gl.useProgram(shaderProgram);

	shaderProgram.vertexPositionAttribute = gl.getAttribLocation(shaderProgram, "aVertexPosition");
	gl.enableVertexAttribArray(shaderProgram.vertexPositionAttribute);

	shaderProgram.textureCoordAttribute = gl.getAttribLocation(shaderProgram, "aTextureCoord");
	gl.enableVertexAttribArray(shaderProgram.textureCoordAttribute);

	shaderProgram.vertexColorAttribute = gl.getAttribLocation(shaderProgram, "aVertexColor");
	gl.enableVertexAttribArray(shaderProgram.vertexColorAttribute);

	shaderProgram.pMatrixUniform = gl.getUniformLocation(shaderProgram, "uPMatrix");
	shaderProgram.mvMatrixUniform = gl.getUniformLocation(shaderProgram, "uMVMatrix");
	shaderProgram.samplerUniform = gl.getUniformLocation(shaderProgram, "uSampler");
}

function degToRad(degrees) {
	return degrees * Math.PI / 180;
}

function handleKeyDown(event) {
	if (event.keyCode == 8) {
		var args = new KeyPressEventArgs();
		args.keyChar = 8;
		keyEventHandler.onKeyPress(args);
		event.stopPropagation();
		event.preventDefault();
	}
	var args = new KeyEventArgs();
	args.keyCode = GetKeyCode(event.keyCode);
	args.setCtrlPressed(event.ctrlKey);
	args.setShiftPressed(event.shiftKey);
	args.setAltPressed(event.altKey);
	keyEventHandler.onKeyDown(args);
	if (args.keyCode == GlKeys.F1 ||
		args.keyCode == GlKeys.F2 ||
		args.keyCode == GlKeys.F3 ||
		args.keyCode == GlKeys.F4 ||
		args.keyCode == GlKeys.F5 ||
		args.keyCode == GlKeys.F6 ||
		args.keyCode == GlKeys.F7 ||
		args.keyCode == GlKeys.F8 ||
		args.keyCode == GlKeys.F9 ||
		args.keyCode == GlKeys.F10 ||
		args.keyCode == GlKeys.F11 ||
		args.keyCode == GlKeys.F12 ||
		args.keyCode == GlKeys.TAB
	) {
		event.stopPropagation();
		event.preventDefault();
		return;
	}
}

function GetKeyCode(jsKey) {
	switch (jsKey) {
		case 8:
			return GlKeys.BACK_SPACE;
		case 9:
			return GlKeys.TAB;
		case 13:
			return GlKeys.ENTER;
		case 16:
			return GlKeys.SHIFT_LEFT;
		case 27:
			return GlKeys.ESCAPE;
		case 32:
			return GlKeys.SPACE;
		case 33:
			return GlKeys.PAGE_UP;
		case 34:
			return GlKeys.PAGE_DOWN;
		case 37:
			return GlKeys.LEFT;
		case 38:
			return GlKeys.UP;
		case 39:
			return GlKeys.RIGHT;
		case 40:
			return GlKeys.DOWN;
		case 48:
			return GlKeys.NUMBER0;
		case 49:
			return GlKeys.NUMBER1;
		case 50:
			return GlKeys.NUMBER2;
		case 51:
			return GlKeys.NUMBER3;
		case 52:
			return GlKeys.NUMBER4;
		case 53:
			return GlKeys.NUMBER5;
		case 54:
			return GlKeys.NUMBER6;
		case 55:
			return GlKeys.NUMBER7;
		case 56:
			return GlKeys.NUMBER8;
		case 57:
			return GlKeys.NUMBER9;
		case 65:
			return GlKeys.A;
		case 66:
			return GlKeys.B;
		case 67:
			return GlKeys.C;
		case 68:
			return GlKeys.D;
		case 69:
			return GlKeys.E;
		case 70:
			return GlKeys.F;
		case 71:
			return GlKeys.G;
		case 72:
			return GlKeys.H;
		case 73:
			return GlKeys.I;
		case 74:
			return GlKeys.J;
		case 75:
			return GlKeys.K;
		case 76:
			return GlKeys.L;
		case 77:
			return GlKeys.M;
		case 78:
			return GlKeys.N;
		case 79:
			return GlKeys.O;
		case 80:
			return GlKeys.P;
		case 81:
			return GlKeys.Q;
		case 82:
			return GlKeys.R;
		case 83:
			return GlKeys.S;
		case 84:
			return GlKeys.T;
		case 85:
			return GlKeys.U;
		case 86:
			return GlKeys.V;
		case 87:
			return GlKeys.W;
		case 88:
			return GlKeys.X;
		case 89:
			return GlKeys.Y;
		case 90:
			return GlKeys.Z;
		case 112:
			return GlKeys.F1;
		case 113:
			return GlKeys.F2;
		case 114:
			return GlKeys.F3;
		case 115:
			return GlKeys.F4;
		case 116:
			return GlKeys.F5;
		case 117:
			return GlKeys.F6;
		case 118:
			return GlKeys.F7;
		case 119:
			return GlKeys.F8;
		case 120:
			return GlKeys.F9;
		case 121:
			return GlKeys.F10;
		case 122:
			return GlKeys.F11;
		case 123:
			return GlKeys.F12;
		default:
			return 0;
	}
}

function handleKeyUp(event) {
	var args = new KeyEventArgs();
	args.keyCode = GetKeyCode(event.keyCode);
	keyEventHandler.onKeyUp(args);
}

function handleKeyPress(event) {
	if (event.charCode == 13) {
		return;
	}
	var args = new KeyPressEventArgs();
	args.keyChar = event.charCode;
	keyEventHandler.onKeyPress(args);
}

function handleMouseDown(event) {
	var args = new MouseEventArgs();
	args.x = event.pageX;
	args.y = event.pageY;
	args.button = event.button;
	mouseEventHandler.onMouseDown(args);
}

function handleMouseUp(event) {
	var args = new MouseEventArgs();
	args.x = event.pageX;
	args.y = event.pageY;
	args.button = event.button;
	mouseEventHandler.onMouseUp(args);
}

var lastMovementX = 0;
var lastMovementY = 0;

function handleMouseMove(event) {
	var args = new MouseEventArgs();
	var movementX = event.movementX ||
		event.mozMovementX ||
		event.webkitMovementX ||
		0;

	var movementY = event.movementY ||
		event.mozMovementY ||
		event.webkitMovementY ||
		0;
	args.movementX = movementX;
	args.movementY = movementY;
	args.forceUsage = true;
	if (!_isMousePointerLocked()) {
		args.x = event.pageX;
		args.y = event.pageY;
	}
	// Bug in Chrome: Sometimes it inserts additional single movement in the opposite direction.
	// To observe this problem, move mouse continuously down. Camera will sometimes look up.
	if ((lastMovementX > 0 && movementX < -100) ||
		(lastMovementX < 0 && movementX > 100) ||
		(lastMovementY > 0 && movementY < -100) ||
		(lastMovementY < 0 && movementY > 100)) {
		// Ignore
	} else {
		mouseEventHandler.onMouseMove(args);
	}
	lastMovementX = movementX;
	lastMovementY = movementY;
}

function handleMouseWheel(event) {
	var args = new MouseWheelEventArgs();
	args.delta = event.wheelDelta / 120;
	args.deltaPrecise = event.wheelDelta / 120;
	mouseEventHandler.onMouseWheel(args);
}


function handleTouchStart(event) {
	var args = new TouchEventArgs();
	for (var i = 0; i < event.changedTouches.length; i++) {
		var touch = event.changedTouches[i];
		args.x = touch.pageX;
		args.y = touch.pageY;
		args.id = touch.identifier;
		touchEventHandler.onTouchStart(args);
	}
	event.stopPropagation();
	event.preventDefault();
}

function handleTouchEnd(event) {
	var args = new TouchEventArgs();

	for (var i = 0; i < event.changedTouches.length; i++) {
		var touch = event.changedTouches[i];
		args.x = touch.pageX;
		args.y = touch.pageY;
		args.id = touch.identifier;
		touchEventHandler.onTouchEnd(args);
	}

	event.stopPropagation();
	event.preventDefault();
}

function handleTouchMove(event) {
	var args = new TouchEventArgs();
	for (var i = 0; i < event.changedTouches.length; i++) {
		var touch = event.changedTouches[i];
		args.x = touch.pageX;
		args.y = touch.pageY;
		args.id = touch.identifier;
		touchEventHandler.onTouchMove(args);
	}
	event.stopPropagation();
	event.preventDefault();
}


var lastTime = 0;
var newFrameArgs = {};
newFrameArgs.dt = 0;
newFrameArgs.getDt = function() {
	return this.dt;
};
newFrameArgs.setDt = function(dt_) {
	this.dt = dt_;
};

var oldCanvasWidth = 0;
var oldCanvasHeight = 0;

function resizeCanvas() {
	if (window.innerWidth != oldCanvasWidth ||
		window.innerHeight != oldCanvasHeight) {
		oldCanvasWidth = window.innerWidth;
		oldCanvasHeight = window.innerHeight;
		canvas.width = window.innerWidth;
		canvas.height = window.innerHeight;
	}
}

function tick() {
	requestAnimFrame(tick);

	resizeCanvas();
	var timeNow = getTimestamp();
	if (lastTime != 0) {
		var elapsed = timeNow - lastTime;
		newFrameArgs.dt = elapsed / 1000;
	}
	lastTime = timeNow;
	newFrameHandler.onNewFrame(newFrameArgs);
}

function webGLStart() {
	canvas = document.getElementById("lesson06-canvas");

	textureCanvas = document.getElementById("textureCanvas");
	textureCanvasContext = textureCanvas.getContext("2d");
	audioContext = new AudioContext();

	initGL(canvas);
	initShaders();

	gl.clearColor(1.0, 0.0, 0.0, 1.0);
	gl.enable(gl.DEPTH_TEST);
	gl.enable(gl.BLEND);
	gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);


	document.onkeydown = handleKeyDown;
	document.onkeyup = handleKeyUp;
	document.onkeypress = handleKeyPress;
	canvas.onmousedown = handleMouseDown;
	document.onmousemove = handleMouseMove;
	document.onmousewheel = handleMouseWheel;
	canvas.onmouseup = handleMouseUp;
	document.addEventListener("touchstart", handleTouchStart, false);
	document.addEventListener("touchmove", handleTouchMove, false);
	document.addEventListener("touchend", handleTouchEnd, false);
	document.addEventListener("contextmenu", function(e) {
		e.preventDefault();
	});

	var mainmenu = new MainMenu();
	var platform = new PlatformJs();
	mainmenu.start(platform);
	platform.start();
	tick();
}
