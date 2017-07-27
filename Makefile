build::
	msbuild clio.sln

release:: build
	mkdir -p dist
	cp -R src/bin/Debug/ dist/
	cp src/clio dist/

clean::
	msbuild /t:clean clio.sln
	rm -rf dist/
