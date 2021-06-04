#!/bin/sh

getpages () {
    for page in $(seq $2 $3); do
        fpage="$(printf "%05d" $page).jpg"
        if [ ! -f $fpage ]; then
            wget "http://diglib.hab.de/mss/$1/max/$fpage"
        fi
    done
}

if [ $# -lt 2 ]; then
    echo "NAME" >&2
    echo "      pget - a tool to mass download OAI pdf pages"
    echo
    echo "SYNOPSIS" >&2
    echo "      $0 [bookid] [pagecount]                    : get the specified amount of pages, starting at page 1" >&2
    echo "      $0 [bookid] [firstpage] [lastpage]         : get all pages between and including the two specified pages" >&2
    echo
    echo "EXAMPLES" >&2
    echo "      $0 ba-1-322 10                             : download the first 10 pages from the book \"ba-1-322\""
    echo "      $0 ba-1-323 100 200                        : download pages 100 to 200 from the book \"ba-1-323\""
    exit 1
elif [ $# -eq 2 ]; then
    getpages $1 0 $2
else
    getpages $1 $2 $3
fi