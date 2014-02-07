mono CodeGenerator.exe Packet.proto

mkdir CitoOutput
mkdir CitoOutput/C
mkdir CitoOutput/Java
mkdir CitoOutput/Cs
mkdir CitoOutput/JsTa

cd CitoOutput/Java
mono ../../CiTo.exe -D CITO -D JAVA -l java -o ManicDigger.java -n ManicDigger.lib  $(ls ../../ManicDiggerLib/Client/*.ci.cs) ../../Packet.Serializer.ci.cs
cd ../..