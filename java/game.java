import java.awt.*;
import java.awt.event.*;

class cube {
    public final static int w = 20, h = 20;
    protected int x, y;
    protected Color color;
    protected int style_id;
    public final static int offset = 60;
    public static int[][] buf;
    public int[][] style;
    public void setColor(Color c) {
        color = c;
    }
    public void drawMain(Graphics g) {
        g.setColor(color);
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                if (style[i][j] == 0)
                    continue;
                g.fillRect(x + j * w + offset, y + i * h, w, h);
            }
        }
    }
    public void drawNext(Graphics g) {
        g.setColor(color);
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                if (style[i][j] == 0)
                    continue;
                g.fillRect(x + j * w, y + i * h, w, h);
            }
        }
    }
    public void setStyle(int id) {
        style = new int[4][4];
        style_id = id;
        switch (id) {
            case 0:
                style[0][0] = 1;
                style[0][1] = 1;
                style[1][0] = 1;
                style[1][1] = 1;
                break;
            case 1:
                style[0][0] = 1;
                style[1][0] = 1;
                style[2][0] = 1;
                style[3][0] = 1;
                break;
            case 2:
                style[0][0] = 1;
                style[1][0] = 1;
                style[0][1] = 1;
                style[0][2] = 1;
                break;
            case 3:
                style[0][0] = 1;
                style[1][0] = 1;
                style[1][1] = 1;
                style[1][2] = 1;
                break;
            case 4:
                style[0][0] = 1;
                style[0][1] = 1;
                style[0][2] = 1;
                style[1][1] = 1;
                break;
            case 5:
                style[0][0] = 1;
                style[1][0] = 1;
                style[1][1] = 1;
                style[2][1] = 1;
                break;
            case 6:
                style[0][1] = 1;
                style[1][1] = 1;
                style[1][0] = 1;
                style[2][0] = 1;
                break;
        }
    }
    public int down() {
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                if (style[i][j] == 1) {
                    try {
                        if (buf[(y + h * (i + 1)) / h][(x + w * j) / w] == 1)
                            return -1;
                    } catch (ArrayIndexOutOfBoundsException e) {
                        if ((y + h * (i + 1)) / h >= 14)
                            return -1;
                        else {
                            if ((x + w * j) / w < 0)
                                x += w;
                            else
                                x -= w;
                        }
                    }
                }
            }
        }
        return y += h;
    }
    public int up() {
        if (y - 1 >= 0)
            return y -= h;
        return -1;
    }
    public int left() {
        for (int j = 3; j >= 0; --j) {
            for (int i = 0; i < 4; ++i) {
                if (style[i][j] == 1) {
                    try {
                        if (buf[(y + h * i) / h][(x + w * (j - 1)) / w] == 1)
                            return -1;
                    } catch (ArrayIndexOutOfBoundsException e) {
                        return -1;
                    }
                }
            }
        }
        return x -= w;
    }
    public int right() {
        for (int j = 0; j < 4; ++j) {
            for (int i = 0; i < 4; ++i) {
                if (style[i][j] == 1) {
                    try {
                        if (buf[(y + h * i) / h][(x + w * (j + 1)) / w] == 1)
                            return -1;
                    } catch (ArrayIndexOutOfBoundsException e) {
                        return -1;
                    }
                }
            }
        }
        return x += w;
    }
    public void setBuf() {
        int left, top;
        left = x / w;
        top = y / h;
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                if (style[i][j] == 1) {
                    try {
                        buf[top + i][left + j] = style[i][j];
                    } catch (ArrayIndexOutOfBoundsException e) {}
                }
            }
        }
    }
    public boolean reset() {
        if (x == 0 && y == 0)
            return false;
        x = 0;
        y = 0;
        return true;
    }
    public void rotate() {
        int tmp[][];
        int cw = 4;
        int left, top;
        left = x / w;
        top = y / h;

        tmp = style;
        style = new int[4][4];

        for (int i = 0; i < cw; ++i)
            for (int j = 0; j < cw; ++j)
                style[cw - 1 - j][i] = tmp[i][j];

        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                if (style[i][j] == 1) {
                    try {
                        if (buf[top + i][left + j] == 1)
                            style = tmp;
                    } catch (ArrayIndexOutOfBoundsException e) {
                        style = tmp;
                    }
                }
            }
        }
    } {
        x = 0;
        y = 0;
        style = new int[4][4];
        buf = new int[14][10];
    }
}
public class game extends Frame {
    protected int score;
    protected int state;
    protected int input;
    class buffer extends Panel implements Runnable {
        cube cube;
        cube nextcube;
        protected int[][] inbuf;
        Image image; 
        Graphics ig;
        {
            cube = new cube();
            nextcube = new cube();
            Color background = new Color(0, 0, 0);
            inbuf = new int[14][10];
            setBackground(background);
            cube.buf = inbuf;
            cube.setStyle((int)(Math.random() * 10) % 7);
            nextcube.setStyle((int)(Math.random() * 10) % 7);
            addKeyListener(new KeyAdapter() {
                public void keyPressed(KeyEvent key) {
                    input = key.getKeyCode();
                }
            });
        }
        public void paint(Graphics gg) {
            if (image == null)
            {
                image = createImage(getSize().width, getSize().height);
                ig = image.getGraphics();
            }
            var g = ig;
            g.clearRect(0, 0, getSize().width, getSize().height);
            if (state == 0) {
                g.setColor(new Color(255, 0, 0));
                g.drawString("Game Over", 100, 100);
                g.setColor(new Color(255, 255, 0));
                g.drawString("After 3s will start a new game...", 50, 150);
            } else {
                init(g);
                drawNext(g);
                drawMain(g);
            }
            gg.drawImage(image, 0, 0, this);
        }
        protected void init(Graphics g) {
            Color border = new Color(255, 255, 255);
            cube.buf = inbuf;
            g.setColor(border);
            g.drawLine(59, 0, 59, 300);
            g.drawLine(59, 280, 260, 280);
            initMain(g);
            drawScore(g);
        }
        protected void initMain(Graphics g) {
            Color b = Color.gray;
            Color c = Color.white;
            for (int i = 0; i < 14; ++i) {
                for (int j = 0; j < 10; ++j) {
                    if (inbuf[i][j] == 0)
                        continue;
                    g.setColor(c);
                    g.fillRect(j * cube.w + cube.offset, i * cube.h, cube.w, cube.h);
                    g.setColor(b);
                    g.drawRect(j * cube.w + cube.offset, i * cube.h, cube.w, cube.h);
                }
            }
        }
        protected void drawNext(Graphics g) {
            nextcube.setColor(new Color(0, 255, 255));
            nextcube.drawNext(g);
        }
        protected void drawMain(Graphics g) {
            cube.setColor(new Color(0, 255, 0));
            cube.drawMain(g);
        }
        protected void drawScore(Graphics g) {
            g.setColor(new Color(255, 255, 255));
            g.drawString("Score : " + score, 70, 300);
        }
        protected void clearCube() {
            boolean is_need_clear = true;
            int ts = 0;
            for (int i = 13; i >= 0; --i) {
                for (int j = 0; j < 10; j++)
                    if (inbuf[i][j] == 0)
                        is_need_clear = false;
                if (is_need_clear) {
                    for (int k = i; k >= 0; --k) {
                        for (int j = 0; j < 10; j++) {
                            try {
                                inbuf[k][j] = inbuf[k - 1][j];
                            } catch (ArrayIndexOutOfBoundsException e) {
                                inbuf[k][j] = 0;
                            }
                        }
                    }
                    i = 14;
                    ++ts;
                }
                is_need_clear = true;
            }
            score += Math.pow(20, ts);
        }
        protected void over() {
            score = 0;
            state = 0;
            inbuf = new int[14][10];
            cube.buf = inbuf;
            nextcube.setStyle((int)(Math.random() * 10) % 7);
        }
        protected void interactive() {
            switch (input) {
                case KeyEvent.VK_LEFT:
                    cube.left();
                    break;
                case KeyEvent.VK_RIGHT:
                    cube.right();
                    break;
                case KeyEvent.VK_UP:
                    cube.rotate();
                    break;
                case KeyEvent.VK_DOWN:
                    if (cube.down() == -1) {
                        cube.setBuf();
                        clearCube();
                        if (cube.reset() == false)
                            over();
                        cube.style = nextcube.style;
                        nextcube.setStyle((int)(Math.random() * 10) % 7);
                    }
                    default:
                        if (cube.down() == -1) {
                            cube.setBuf();
                            clearCube();
                            if (cube.reset() == false)
                                over();
                            cube.style = nextcube.style;
                            nextcube.setStyle((int)(Math.random() * 10) % 7);
                        }
                        break;
            }
            input = KeyEvent.VK_UNDEFINED;
        }
        public synchronized void run() {
            while (true) {
                try {
                    wait(300);
                } catch (InterruptedException e) {}
                interactive();
                repaint();
                if (state == 0) {
                    try {
                        wait(3000);
                    } catch (InterruptedException e) {}
                    state = 1;
                }
            }
        }
    }
    buffer buf; {
        buf = new buffer();
        addWindowListener(new WindowAdapter() {
            public void windowClosing(WindowEvent e) {
                System.exit(0);
            }
        });
        setSize(280, 340);
        score = 0;
        state = 1;
        input = KeyEvent.VK_UNDEFINED;
        add(buf);
    }
    public static void main(String[] args) {
        game myFrame = new game();
        myFrame.setVisible(true);
        (new Thread(myFrame.buf)).start();
    }
}
