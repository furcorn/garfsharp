#Missfield, for missing strips from Massfield
from pathlib import Path
import time
import datetime
from os import path
print("""┌───────────────────────────────────┐
│             Missfield             │
│ ───────────────────────────────── │
│ for missing strips from Massfield │
└───────────────────────────────────┘""")
mindate = datetime.date(year=1986,month=3,day=3)
maxdate = datetime.date(year=1989,month=5,day=7)
days = (maxdate-mindate).days
dateformat = "usa%Y-%m-%d.gif"
file = open("missing.txt", "w")
for i in range(days+1):
	currentdate = mindate + datetime.timedelta(days=i)
	if path.isfile(currentdate.strftime(dateformat)) is not True:
		print("You're missing "+currentdate.strftime("%Y-%m-%d"))
		file.write(currentdate.strftime("%Y-%m-%d"))
		if (i >= days-1) is not True:
			file.write("\n")
file.close()
print("Finished writing missing.txt")