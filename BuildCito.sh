mono CodeGenerator.exe Packet.proto

rm Html/*.js

mono CitoAssets.exe data Assets.ci.cs
mono CiTo.exe -D CITO -D JS -D JSTA -l js-ta -o Html/Assets.js Assets.ci.cs
mono CiTo.exe -D CITO -D JS -D JSTA -l js-ta -o Html/ManicDigger.js $(ls ManicDiggerLib/Client/*.ci.cs) $(ls ManicDiggerLib/Client/Mods/*.ci.cs) $(ls ManicDiggerLib/Client/MainMenu/*.ci.cs) $(ls ManicDiggerLib/Client/Misc/*.ci.cs) Packet.Serializer.ci.cs
