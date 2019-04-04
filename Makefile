build::
	msbuild clio.sln

prepare::
	nuget restore clio.sln

release:: build
	mkdir -p dist
	cp -R src/bin/Debug/ dist/
	cp src/clio dist/

chris-install:: release
	rm -rf ~/bin/clio/
	mkdir -p ~/bin/clio/
	cp -R dist/* ~/bin/clio/

clean::
	msbuild /t:clean clio.sln
	rm -rf dist/
