public class ShaderCi
{
	GamePlatform p;
	bool isLinked;
	GlProgram programId;

	public ShaderCi()
	{
		isLinked = false;
		programId = null;
		p = null;
	}

	/// <summary>
	/// Initialize the class. CiTo does not support parameters for constructors.
	/// </summary>
	/// <param name="platform"></param>
	public void Init(GamePlatform platform)
	{
		p = platform;
		programId = p.GlCreateProgram();
	}

	/// <summary>
	/// Free allocated resources on the graphics card.
	/// </summary>
	public void Dispose()
	{
		if (programId != null)
		{
			p.GlDeleteProgram(programId);
		}
		p = null;
	}

	/// <summary>
	/// Compile a specific type of shader given the source as a string.
	/// </summary>
	/// <param name="sShader">Shader source code</param>
	/// <param name="type">Shader type to create</param>
	public void Compile(string sShader, ShaderType type)
	{
		isLinked = false;
		GlShader shaderObject = p.GlCreateShader(type);
		if (null == shaderObject) { p.ThrowException("Could not create shader object"); }
		p.GlShaderSource(shaderObject, sShader);
		p.GlCompileShader(shaderObject);
		if (!p.GlGetShaderCompileStatus(shaderObject))
		{
			p.ThrowException(p.StringFormat("Error compiling shader: \n{0}", p.GlGetShaderInfoLog(shaderObject)));
		}
		p.GlAttachShader(programId, shaderObject);
	}

	/// <summary>
	/// Begin using this shader.
	/// </summary>
	public void BeginUse()
	{
		if (programId == null) { return; }
		p.GlUseProgram(programId);
	}

	/// <summary>
	/// End using this shader.
	/// </summary>
	public void EndUse()
	{
		if (programId == null) { return; }
		p.GlUseProgram(null);
	}

	/// <summary>
	/// Get location of specified uniform variable.
	/// </summary>
	/// <param name="name">Uniform variable name</param>
	/// <returns>Uniform variable location</returns>
	public int GetUniformLocation(string name)
	{
		return p.GlGetUniformLocation(programId, name);
	}

	/// <summary>
	/// Determine if the shader has been successfully linked.
	/// </summary>
	/// <returns>true if the shader is linked and ready to use, false otherwise</returns>
	public bool IsLinked()
	{
		return isLinked;
	}

	/// <summary>
	/// Link the program using previously attached shaders.
	/// </summary>
	public void Link()
	{
		p.GlLinkProgram(programId);
		if (!p.GlGetProgramLinkStatus(programId))
		{
			p.ThrowException(p.StringFormat("Error linking shader: \n{0}", p.GlGetProgramInfoLog(programId)));
		}
		isLinked = true;
	}
}

public class ShaderSources
{
	public const string FragmentSolidColor =
		"#version 110\n" +
		"void main()" +
		"{" +
			"gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);" +
		"}";

	public const string VertexPassthrough =
		"#version 110\n" +
		"void main(void)" +
		"{" +
			"gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;" +
		"}";

	public const string FragmentCopy =
		"#version 130\n" +
		"uniform sampler2D image;" +
		"in vec2 uv;" +
		"void main()" +
		"{" +
			"vec3 image = texture(image, uv).rgb;" +
			"gl_FragColor = vec4(image, 1.0);" +
		"}";

	public const string VertexCreate =
		"#version 130\n" +
		"out vec2 uv;" +
		"void main()" +
		"{" +
			"const vec2 vertices[4] = vec2[4](vec2(-1.0, -1.0)," +
											 "vec2(1.0, -1.0)," +
											 "vec2(1.0, 1.0)," +
											 "vec2(-1.0, 1.0));" +
			"vec2 pos = vertices[gl_VertexID];" +
			"uv = pos * 0.5 + 0.5;" +
			"gl_Position = vec4(pos, 1.0, 1.0);" +
		"}";

	public const string FragmentSkysphere =
		"#version 110\n" +
		"uniform sampler2D stars;" +
		"uniform sampler2D color;" +
		"uniform sampler2D glow;" +
		"uniform vec3 sunPos;" +
		"varying vec3 vertex;" +
		"void main()" +
		"{" +
			"vec3 V = normalize(vertex);" +
			"vec3 L = normalize(sunPos);" +
			"float vl = dot(V, L);" +
			"vec4 Kc = texture2D(color, vec2((L.y + 1.0) / 2.0, 1.0 - (V.y + 1.0) / 2.0));" +
			"vec4 Kg = texture2D(glow, vec2((L.y + 1.0) / 2.0, 1.0 - (vl + 1.0) / 2.0));" +
			"vec4 St = texture2D(stars, vec2(V.x, V.z));" +
			"gl_FragColor = mix(St, vec4(Kc.rgb + Kg.rgb * Kg.a / 2.0 , Kc.a), Kc.a);" +
		"}";

	public const string VertexSkysphere =
		"#version 110\n" +
		"varying vec3 vertex;" +
		"void main()" +
		"{" +
			"vertex = gl_Vertex.xyz;" +
			"gl_Position = ftransform();" +
		"}";
}

public enum ShaderType
{
	VertexShader,
	FragmentShader
}
