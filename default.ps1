properties {
    $buildDir = ".\build"
    $outputDir = $buildDir + "\lib\" + $framework
}

task default -depends Compile, Clean

task Compile -depends Clean { 
    $sources = gci ".\src" -r -fi *.cs |% { $_.FullName }
    mkdir $outputDir
    $out = $outputDir + "\wordbox.dll"
    csc /target:library /out:$out $sources /keyfile:src\Wordbox.snk
}

task Clean { 
    if (test-path $outputDir) { ri -r -fo $outputDir }
}
