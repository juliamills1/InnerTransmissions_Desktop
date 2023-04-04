// sound file
me.sourceDir() + "radio6.wav" => string filename;

// the patch 
SndBuf buf => dac;
// load the file
filename => buf.read;

// time loop
while (true)
{
    0 => buf.pos;
    buf.length() => now;
}
