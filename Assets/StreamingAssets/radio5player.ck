// sound file
me.sourceDir() + "radio5.wav" => string filename;

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
