perseus-plugins
===============

[![NuGet version](https://badge.fury.io/nu/PerseusApi.svg)](https://www.nuget.org/profiles/coxgroup)

Perseus is a software framework for the statistical analysis of omics data. It has a plugin architecture which allows users to contribute their own data analysis activities. Here you find the code necessary to develop your own plugins. 

alternative?
===============

1. " git clone https://github.com/JurgenCox/perseus-plugins.git "
2. open solution in visual studio
3. right click on the "Solution" and "Add a new project" of "Class Library(.NET Standard) name starting with "Plugin" something like “Plugin<NameofTheModule>” (e.g https://github.com/animesh/perseus-plugins/tree/master/PluginQNorm/)
4. add reference (PerseusApi, BaseLibS) from DLLs folder
5. right click on the created solution  and select “build” or work with an existing code, for example is a simple code for row/column quantile normalization from perseus interface
7. copy the created “dll” from the <path-to-solution\bin\Debug\ to   <path-to-perseus>\bin\

