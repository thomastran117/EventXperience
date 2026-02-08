import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type TimelineItem = {
  title: string;
  desc?: string;
  meta?: string;
};

@Component({
  selector: 'timeline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './timeline.component.html',
  styleUrl: './timeline.component.css',
})
export class TimelineComponent {
  @Input({ required: true }) items!: TimelineItem[];
  @Input() className = '';
}
