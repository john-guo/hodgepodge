extern crate sdl2; 
extern crate rand;

use sdl2::pixels::Color;
use sdl2::event::Event;
use sdl2::keyboard::Keycode;
use sdl2::ttf::Font;
use std::time::Duration;
use sdl2::rect::Rect;
 
#[macro_export]
macro_rules! var {
    ($($i:ident, $x:expr),*) => {
        $(
            let mut $i = $x;
        )*
    };
}

const w : u32 = 20;
const h : u32 = 20;
const _ROWS : usize = 14;
const _COLS : usize = 10;
static ROWS : u32 = 14;
static COLS : u32 = 10;
static mut buf : [[u32; _COLS]; _ROWS] = [[0; _COLS]; _ROWS];
static mut offset : u32 = 60;

struct cube {
    map : [[u8;4];4],
    x : u32,
    y : u32,
    id : u8,
    color: Color
}

impl cube {
    fn new() -> cube 
    {
        cube {
            map : [[0; 4]; 4],
            x : 0,
            y : 0,
            id : 0,
            color : Color::BLACK
        }
    }

    fn setColor(&mut self, c : Color) {
        self.color = c;
    }

    fn drawMain(&mut self, canvas : &mut sdl2::render::WindowCanvas) 
    {
        canvas.set_draw_color(self.color);

        for i in 0..4 
        {
            for j in 0..4 
            {
                if self.map[i][j] == 0
                {
                    continue;
                }

                let u : u32 = j.try_into().unwrap();
                let v : u32 = i.try_into().unwrap();

                unsafe {
                    let u : u32 = self.x + u * w + offset;
                    let v : u32 = self.y + v * h;
    
                    canvas.fill_rect(Rect::new(u.try_into().unwrap(), v.try_into().unwrap(), w, h));
                }
            }
        }
    }

    fn drawNext(&mut self, canvas : &mut sdl2::render::WindowCanvas) 
    {
        canvas.set_draw_color(self.color);

        for i in 0..4 
        {
            for j in 0..4 
            {
                if self.map[i][j] == 0
                {
                    continue;
                }

                let u : u32 = j.try_into().unwrap();
                let v : u32 = i.try_into().unwrap();

                let u : u32 = self.x + u * w;
                let v : u32 = self.y + v * h;

                canvas.fill_rect(Rect::new(u.try_into().unwrap(), v.try_into().unwrap(), w, h));
            }
        }
    }

    fn make(&mut self, id: u8) {
        self.map = [[0; 4]; 4];
        match id {
            0 => {
                self.map[0][0] = 1;
                self.map[0][1] = 1;
                self.map[1][0] = 1;
                self.map[1][1] = 1;
            },
            1 => {
                self.map[0][0] = 1;
                self.map[1][0] = 1;
                self.map[2][0] = 1;
                self.map[3][0] = 1;
            },
            2 => {
                self.map[0][0] = 1;
                self.map[1][0] = 1;
                self.map[0][1] = 1;
                self.map[0][2] = 1;
            },
            3 => {
                self.map[0][0] = 1;
                self.map[1][0] = 1;
                self.map[1][1] = 1;
                self.map[1][2] = 1;
            },
            4 => {
                self.map[0][0] = 1;
                self.map[0][1] = 1;
                self.map[0][2] = 1;
                self.map[1][1] = 1;
            },
            5 => {
                self.map[0][0] = 1;
                self.map[1][0] = 1;
                self.map[1][1] = 1;
                self.map[2][1] = 1;
            },
            6 => {
                self.map[0][1] = 1;
                self.map[1][1] = 1;
                self.map[1][0] = 1;
                self.map[2][0] = 1;
            },
            _ => {

            }
        }
    }
    fn down(&mut self) -> i32 {
        for i in 0..4 {
            for j in 0..4 {
                let x:u32 = i.try_into().unwrap();
                let y:u32 = j.try_into().unwrap();
                if self.map[i][j] == 1 {
                    let mut row:u32 = (self.y + h * (x + 1)) / h;
                    let mut col:u32 = (self.x + w * y) / w;
                    
                    if row >= ROWS || col >= COLS
                    {
                        if row >= ROWS
                        {
                            return -1;
                        }
                        else 
                        {
                            if col < 0
                            {
                                col = 0;
                                self.x += w;
                            }
                            else if col >= COLS
                            {
                                col = COLS - 1;
                                self.x -= w;
                            }
                        }
                    }
                    
                    let u:usize = row.try_into().unwrap();
                    let v:usize = col.try_into().unwrap();
                    unsafe {
                        if buf[u][v] == 1
                        {
                            return -1;   
                        }
                    }
                }
            }
        }
        self.y += h;
        return 0;
    }
    fn up(&mut self) {
        if self.y - 1 >= 0
        {
            self.y -= h;
        }
        -1;
    }
    fn left(&mut self) -> i32 {
        for j in 4..0 {
            for i in 0..4 {
                let x:u32 = i.try_into().unwrap();
                let y:u32 = j.try_into().unwrap();

                if self.map[i][j] == 1 {
                    let row:u32 = (self.y + h * x) / h;
                    let col:u32 = (self.x + w * (y - 1)) / w;
                    if row >= ROWS || col >= COLS
                    {
                        return -1;
                    }

                    let u:usize = row.try_into().unwrap();
                    let v:usize = col.try_into().unwrap();
                    unsafe {
                        if buf[u][v] == 1
                        {
                            return -1;   
                        }
                    }
                }
            }
        }
        
        if self.x >= w
        {
            self.x -= w;
        }

        return 0;
    }

    fn right(&mut self) -> i32 {
        for j in 0..4 {
            for i in 0..4 {
                let x:u32 = i.try_into().unwrap();
                let y:u32 = j.try_into().unwrap();
                if self.map[i][j] == 1 {
                    let row:u32 = (self.y + h * x) / h;
                    let col:u32 = (self.x + w * (y + 1)) / w;
                    if row >= ROWS || col >= COLS
                    {
                        return -1;
                    }

                    let u:usize = row.try_into().unwrap();
                    let v:usize = col.try_into().unwrap();
                    unsafe {
                        if buf[u][v] == 1
                        {
                            return -1;   
                        }
                    }
                }
            }
        }
        self.x += w;

        return 0;
    }
    fn setBuf(&mut self) {
        let left = self.x / w;
        let top = self.y / h;
        for i in 0..4 {
            for j in 0..4 {
                let x:u32 = i.try_into().unwrap();
                let y:u32 = j.try_into().unwrap();
                if self.map[i][j] == 1 {
                    let row:u32 = top + x;
                    let col:u32 = left + y;
                    if row < ROWS && col < COLS
                    {
                        let u:usize = row.try_into().unwrap();
                        let v:usize = col.try_into().unwrap();

                        unsafe 
                        {
                            buf[u][v] = self.map[i][j].try_into().unwrap();
                        }
                    }
                }
            }
        }
    }
    fn reset(&mut self) -> bool {
        if self.x == 0 && self.y == 0
        {
            return false;
        }
        self.x = 0;
        self.y = 0;
        return true;
    }
    fn rotate(&mut self) {
        let tmp = self.map.clone();
        let cw = 4;
        let left = self.x / w;
        let top = self.y / h;

        self.map = [[0;4]; 4];

        for i in 0..cw {
            for j in 0..cw {
                self.map[cw - 1 - j][i] = tmp[i][j];
            }
        }

        for i in 0..4 {
            for j in 0..4 {
                let x:u32 = i.try_into().unwrap();
                let y:u32 = j.try_into().unwrap();
                if self.map[i][j] == 1 {
                    let row:u32 = top + x;
                    let col:u32 = left + y;
                    if row < ROWS && col < COLS
                    {
                        let u:usize = row.try_into().unwrap();
                        let v:usize = col.try_into().unwrap();
                        unsafe {
                            if buf[u][v] == 1
                            {
                                self.map = tmp.clone(); 
                            }
                        }
                    }
                }
            }
        }
    }
}

struct game {
    current : cube,
    next : cube,
    score : i32,
    state : i32,
}

impl game {
    fn new() -> game {
        let mut g = game {
            current : cube::new(),
            next : cube::new(),
            score : 0,
            state : 1,
        };

        g.current.make(rand::random::<u8>() % 7);
        g.next.make(rand::random::<u8>() % 7);

        return g;
    }

    fn draw(&mut self, canvas : &mut sdl2::render::WindowCanvas, font : &Font)
    {
        canvas.set_draw_color(Color::RGB(0, 0, 0));
        canvas.clear();
        if self.state == 0
        {
            canvas.set_draw_color(Color::RGB(255, 0, 0));
            let surface = font
                        .render("Game Over")
                        .blended(Color::RGB(255, 0, 0)).unwrap();
            let texture_creator = canvas.texture_creator();
            let texture = texture_creator.create_texture_from_surface(&surface).unwrap();
            let sdl2::render::TextureQuery { width, height, .. } = texture.query();
            canvas.copy(&texture, None, Some(Rect::new(100, 100, width, height)));

            canvas.set_draw_color(Color::RGB(255, 255, 0));
            let surface = font
                        .render("Press any key start a new game...")
                        .blended(Color::RGB(255, 255, 0)).unwrap();
            let texture_creator = canvas.texture_creator();
            let texture = texture_creator.create_texture_from_surface(&surface).unwrap();
            let sdl2::render::TextureQuery { width, height, .. } = texture.query();
            canvas.copy(&texture, None, Some(Rect::new(50, 50, width, height)));

        } else {
            self.init(canvas, font);
            self.drawNext(canvas);
            self.drawMain(canvas);
        }
    }

    fn init(&mut self, canvas : &mut sdl2::render::WindowCanvas, font : &Font)
    {
        let border = Color::RGB(255, 255, 255);

        canvas.set_draw_color(border);
        canvas.draw_line(sdl2::rect::Point::new(59, 0),  sdl2::rect::Point::new(59, 300));
        canvas.draw_line(sdl2::rect::Point::new(59, 280),  sdl2::rect::Point::new(260, 280));

        self.initMain(canvas);
        self.drawScore(canvas, font);
    }

    fn initMain(&mut self, canvas : &mut sdl2::render::WindowCanvas)
    {
        let b = Color::GRAY;
        let c = Color::WHITE;
        for i in 0..14
        {
            for j in 0..10 
            {
                unsafe {

                    if buf[i][j] == 0
                    {
                        continue;
                    }
    
                    let u : u32 = j.try_into().unwrap();
                    let v : u32 = i.try_into().unwrap();
    
                    let r = Rect::new((u * w + offset).try_into().unwrap(), (v * h).try_into().unwrap(), w, h);
                    
                    canvas.set_draw_color(c);
                    canvas.fill_rect(r);
    
                    canvas.set_draw_color(b);
                    canvas.draw_rect(r);
    
                }
            }
        }
    }

    fn drawNext(&mut self, canvas : &mut sdl2::render::WindowCanvas)
    {
        self.next.setColor(Color::RGB(0, 255, 255));
        self.next.drawNext(canvas);
    }

    fn drawMain(&mut self, canvas : &mut sdl2::render::WindowCanvas)
    {
        self.current.setColor(Color::RGB(0, 255, 0));
        self.current.drawMain(canvas);
    }

    fn drawScore(&mut self, canvas : &mut sdl2::render::WindowCanvas, font : &Font)
    {
        canvas.set_draw_color(Color::RGB(255, 255, 255));

        let str = &format!("Score : {}", self.score);
        let surface = font.render(str).blended(Color::RGB(255, 255, 255)).unwrap();
        let texture_creator = canvas.texture_creator();
        let texture = texture_creator.create_texture_from_surface(&surface).unwrap();
        let sdl2::render::TextureQuery { width, height, .. } = texture.query();
        canvas.copy(&texture, None, Some(Rect::new(70, 300, width, height)));
    }

    fn clearCube(&mut self)
    {
        let mut is_need_clear : bool = true;
        let mut ts : u32 = 0;
        let mut i : i32 = 13;
        while i >= 0
        {
            for j in 0..10 {
                let u:usize = i.try_into().unwrap();
                let v:usize = j.try_into().unwrap();

                unsafe {
                    if buf[u][v] == 0
                    {
                        is_need_clear = false;
                    }
                }
            }
            if is_need_clear
            {
                let mut k = i;
                while k >= 0 {
                    for j in 0..10 {
                        let u:usize = k.try_into().unwrap();
                        let v:usize = j.try_into().unwrap();
                        unsafe {
                            if k == 0
                            {
                                buf[u][v] = 0;
                            }
                            else
                            {
                                buf[u][v] = buf[u - 1][v];
                            }
                        }
                    }
                    k -= 1;
                }
                i = 13;
                ts+=1;
            }
            is_need_clear = true;
            i -= 1;
        }
        self.score += i32::pow(20, ts);
    }

    fn over(&mut self,)
    {
        self.score = 0;
        self.state = 0;
        unsafe {
            buf  = [[0; _COLS]; _ROWS];
        }
        self.next.make(rand::random::<u8>() % 7);
    }

    fn interactive(&mut self, input : Event) -> bool
    {
        match input {
            Event::KeyDown { keycode: Some(Keycode::Left), .. } => {
                self.current.left();
            },
            Event::KeyDown { keycode: Some(Keycode::Right), .. } => {
                self.current.right();
            },
            Event::KeyDown { keycode: Some(Keycode::Up), .. } => {
                self.current.rotate();
            },

            Event::KeyDown { keycode: Some(Keycode::Down), .. } => {
                
                if self.current.down() == -1
                 {
                    self.current.setBuf();
                    self.clearCube();
                    if self.current.reset() == false
                    {
                        self.over();
                    }
                    self.current.map = self.next.map;
                    self.next.make(rand::random::<u8>() % 7);
                }
            },

            Event::KeyDown { keycode: Some(Keycode::Escape), .. } => {
                return false;
            },

            _ => {
                if self.state == 0 
                {
                    self.state = 1;
                }
            }
        }
        return true;
    }

    fn update(&mut self)
    {
        if self.current.down() == -1 
        {
            self.current.setBuf();
            self.clearCube();
            if self.current.reset() == false
            {
                self.over();
            }
            self.current.map = self.next.map;
            self.next.make(rand::random::<u8>() % 7);
        }
    }
}


pub fn main() {
    let sdl_context = sdl2::init().unwrap();
    let video_subsystem = sdl_context.video().unwrap();
 
    let ttf = sdl2::ttf::init().unwrap();
    let font = &ttf.load_font(std::path::Path::new("arial.ttf"), 16).unwrap();

    let window = video_subsystem.window("my Rust Tetris Democ", 800, 600)
        .position_centered()
        .build()
        .unwrap();
 
    let canvas = &mut window.into_canvas().build().unwrap();
 
    let mut gam : game = game::new();
    
    canvas.set_draw_color(Color::RGB(0, 0, 0));
    canvas.clear();
    canvas.present();
    let mut ticket = 0;
    let mut event_pump = sdl_context.event_pump().unwrap();
    'eventloop: loop {

        gam.draw(canvas, font);

        if ticket % 30 == 0
        {
            gam.update();
        }

        for event in event_pump.poll_iter() {
            if gam.interactive(event) == false
            {
                break 'eventloop;
            }
        }

        // The rest of the game loop goes here...

        ticket += 1;
        canvas.present();
        ::std::thread::sleep(Duration::new(0, 1_000_000_000u32 / 60));
    }
}