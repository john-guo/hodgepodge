windows下学习Rust的第一个小程序，俄罗斯方块，会存在一些奇怪的bug。


编译

cargo build

由于使用sdl2,sdl2_ttf，编译时需要把SDL2.lib SDL2_ttf.lib复制到rust安装的toolchains lib中


运行

cargo run

运行时需要复制字体文件 arial.ttf, 以及动态库 sdl2.dll, sdl2_ttf.dll, libfreetype-6.dll, zlib1.dll 到此目录。
