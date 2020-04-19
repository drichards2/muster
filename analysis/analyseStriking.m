% function analyseStriking(logfile)
logfile = "C:\Users\jagg\Downloads\Muster\muster-debug-log.txt";

log = fileread(logfile);
lines = strsplit(log, '\n');
lines = strtrim(lines);

idxConnect = find(cellfun(@(s) ~isempty(s), strfind(lines, "Connecting to")));

RX = arrayfun(@(l) regexp(l, '^(?<timestamp>[^|]+).+Received "(?<bell>[A-P])', 'names'), lines, 'UniformOutput', false);
RX(cellfun(@(s) isempty(s{1}), RX)) = [];
RX = [RX{:}];
RX = [RX{:}];
for idx = 1:numel(RX)
    RX(idx).bell = RX(idx).bell - 'A' + 1;
    RX(idx).timestamp = datetime(RX(idx).timestamp);
end

TX = arrayfun(@(l) regexp(l, '^(?<timestamp>[^|]+).+Key press:.+\-> (?<bell>\d+)', 'names'), lines, 'UniformOutput', false);
TX(cellfun(@(s) isempty(s{1}), TX)) = [];
TX = [TX{:}];
TX = [TX{:}];
for idx = 1:numel(TX)
    TX(idx).bell = TX(idx).bell - '0';
    TX(idx).timestamp = datetime(TX(idx).timestamp);
end

%%
analysisStart = datetime('2020-04-16 18:19:49.9615');
analysisEnd = datetime('2020-04-16 18:25:11.8');

RXtouch = RX([RX.timestamp] >= analysisStart & [RX.timestamp] < analysisEnd);
TXtouch = TX([TX.timestamp] >= analysisStart & [TX.timestamp] < analysisEnd);

allBlows = [RXtouch, TXtouch];
[~, idxSort] = sort([allBlows.timestamp]);
allBlows = allBlows(idxSort);

%%
strikes = reshape([allBlows.bell, 6]', 6, [])';

%%
strikesSorted = sort(strikes,2);
find(arrayfun(@(i) any(strikesSorted(i,:) ~= (1:6)), 1:size(strikesSorted,1)))

%%
allBlowsFixed = [allBlows(1:61*6-1), struct('timestamp', NaT, 'bell', 6), allBlows(61*6:end)];
t = fillmissing([allBlowsFixed(:).timestamp], 'linear');
max([allBlowsFixed.timestamp] - t)
for idx = 1:numel(allBlowsFixed)
    allBlowsFixed(idx).timestamp = t(idx);
end

%%
strikeTimes = reshape([allBlowsFixed.timestamp]', 6, [])';
strikeTimes = strikeTimes - datetime(strikeTimes(1).Year, strikeTimes(1).Month, strikeTimes(1).Day);
strikeTimes.Format = 'hh:mm:ss.SSS';

%%
strikes = reshape([allBlowsFixed.bell]', 6, [])'

%%
deltas = diff([allBlowsFixed.timestamp]);
deltas = reshape([seconds(deltas(:));NaN], 6, [])'

%%
hist(deltas(:), 50)