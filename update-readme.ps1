# Update README.md to add reference to ternary computing documentation

$content = Get-Content 'e:/Projects/t3_sharp/README.md' -Raw
$pattern = '(\| \[docs/t3-isa-reference\.ru\.md\]\(docs/t3-isa-reference\.ru\.md\) \| Russian \| Complete instruction set reference \|)(\r\n\r\n)'
$replacement = '$1`n| [docs/ternary-computing-documentation.md](docs/ternary-computing-documentation.md) | English | Scientific documentation on ternary computing (balanced ternary math, arithmetic, logic) |$2'
$content = $content -replace $pattern, $replacement
Set-Content -Path 'e:/Projects/t3_sharp/README.md' -Value $content -NoNewline

Write-Host "README.md updated successfully"
