CC=gcc
CXX=g++
RM=rm -f
CXXFLAGS= -Wall -O3 -Ofast -mcpu=cortex-a53 -mtune=cortex-a53 -g -std=c++17 -ITubeTables -IClipping -I../eigen
LDFLAGS=
LDLIBS= /usr/lib/arm-linux-gnueabihf/libjack.so -lpthread -lasound -lstdc++fs
PREFIX= /usr/local

CPPFILES=$(filter-out GuitarSim.cpp GuitarSimAlsa.cpp, $(wildcard *.cpp)) $(wildcard WDL/*.cpp) $(wildcard NAM/*.cpp) 
CFILES=$(wildcard *.c) $(wildcard WDL/*.c)
OBJS=$(subst .cpp,.o,$(CPPFILES)) $(subst .c,.o,$(CFILES))

all: GuitarSim GuitarSimAlsa


.PHONY: install
install: GuitarSim
	install -m 0755 GuitarSim $(PREFIX)/bin
	install -d $(PREFIX)/bin/Cabinets
	install -m 0644 Cabinets/* $(PREFIX)/bin/Cabinets

GuitarSim: $(OBJS) GuitarSim.o
	$(CXX) $(LDFLAGS) -o GuitarSim GuitarSim.o $(OBJS) $(LDLIBS)

GuitarSimAlsa: $(OBJS) GuitarSimAlsa.o
	$(CXX) $(LDFLAGS) -o GuitarSimAlsa GuitarSimAlsa.o $(OBJS) $(LDLIBS)

depend: .depend

.depend: $(SRCS)
	$(RM) ./.depend
	$(CXX) $(CXXFLAGS) -MM $^>>./.depend;

clean:
	$(RM) $(OBJS)
	$(RM) GuitarSim GuitarSimAlsa GuitarSim.o GuitarSimAlsa.o

distclean: clean
	$(RM) *~ .depend

include .depend
