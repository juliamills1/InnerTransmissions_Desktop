// ---- FOOTSTEPS ---- //
global Event changeHappened;
// movement: 0 = stationary, 1 = walk
global float mode;
me.sourceDir() + "crunchySteps.wav" => string walking;
SndBuf buf => Gain g => dac;
1 => buf.loop;
1.5 => g.gain;

// footsteps according to player movement
while (true)
{
    // movement mode has changed
    changeHappened => now;
    0 => buf.pos;
    
    if (mode == 0)
    {
        // stationary: silent
        0 => buf.gain;
    }
    else if (mode == 1)
    {
        // use walk file in buffer
        walking => buf.read;
        0.8 => buf.gain;
    }
}
