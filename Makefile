build::
	dotnet build

release::
	dotnet publish
	mkdir -p dist
	cp -R src/Clio/bin/Debug/net5.0/publish/ dist/
	cp tools/Clio dist/Clio

chris-install:: release
	rm -rf ~/bin/clio/
	mkdir -p ~/bin/clio/
	cp -R dist/* ~/bin/clio/

clean::
	dotnet clean 
	rm -rf dist/
