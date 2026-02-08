import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './section.component.html',
  styleUrl: './section.component.css',
})
export class SectionComponent {
  @Input() title?: string;
  @Input() subtitle?: string;

  @Input() className = '';
  @Input() contentClass = 'mt-6';
}
