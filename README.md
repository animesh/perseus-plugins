perseus-plugins
===============

[![NuGet version](https://badge.fury.io/nu/PerseusApi.svg)](https://www.nuget.org/profiles/coxgroup)

Perseus is a software framework for the statistical analysis of omics data. It has a plugin architecture which allows users to contribute their own data analysis activities. Here you find the code necessary to develop your own plugins. 

Setup

1. " git clone https://github.com/JurgenCox/perseus-plugins.git "
2. open solution in visual studio
3. create a class with “Plugin” prefixed
4. add reference (PerseusApi, BaseLibS)
5. create default methods as suggested or copy from existing code, for example https://raw.githubusercontent.com/animesh/perseus-plugins/master/PluginANN/BackProp.cs is a simple code with accepts a factor from perseus interface and multiplies all the values from the input matrix by that factor
6. right click on the created class “Plugin<NameofTheModule>” and select “build”
7. copy the created “dll” from the <path-to-solution\bin\Debug\ to   <path-to-perseus>\bin\

